using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Medidores;

namespace WebZootecPro.Controllers
{
    public class MedidoresController : Controller
    {
        private readonly ZootecContext _context;
        public MedidoresController(ZootecContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, bool? procesado)
        {
            var q = _context.LecturaMedidorLeches.AsQueryable();

            if (desde.HasValue) q = q.Where(x => x.FechaHoraLectura >= desde.Value);
            if (hasta.HasValue) q = q.Where(x => x.FechaHoraLectura <= hasta.Value);
            if (procesado.HasValue) q = q.Where(x => x.Procesado == procesado.Value);

            var items = await q.OrderByDescending(x => x.FechaHoraLectura)
                               .ThenByDescending(x => x.IdLecturaMedidorLeche)
                               .Take(500)
                               .ToListAsync();

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Procesado = procesado;

            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View(new LecturaMedidorVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LecturaMedidorVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var entity = new LecturaMedidorLeche
            {
                CodigoMedidor = vm.CodigoMedidor,
                CodigoAnimal = vm.CodigoAnimal,
                FechaHoraLectura = vm.FechaHoraLectura,
                PesoLecheKg = vm.PesoLecheKg,
                NumeroOrdeno = vm.NumeroOrdeno,
                Procesado = false,
                Observacion = vm.Observacion
            };

            _context.LecturaMedidorLeches.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Procesa lecturas pendientes:
        // - intenta mapear animal por CodigoAnimal comparando con: Animal.codigo / IdentificadorElectronico / OtroIdentificador
        // - crea o actualiza RegistroProduccionLeche por (idAnimal + fecha + turno)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarPendientes(int max = 200)
        {
            var pendientes = await _context.LecturaMedidorLeches
                .Where(x => !x.Procesado)
                .OrderBy(x => x.FechaHoraLectura)
                .Take(max)
                .ToListAsync();

            int ok = 0, sinAnimal = 0;

            foreach (var lec in pendientes)
            {
                // 1) Resolver IdAnimal
                int? idAnimal = lec.IdAnimal;

                if (!idAnimal.HasValue && !string.IsNullOrWhiteSpace(lec.CodigoAnimal))
                {
                    var cod = lec.CodigoAnimal.Trim();

                    var animal = await _context.Animals.FirstOrDefaultAsync(a =>
                        a.codigo == cod ||
                        a.IdentificadorElectronico == cod ||
                        a.OtroIdentificador == cod
                    );

                    if (animal != null)
                    {
                        idAnimal = animal.Id;
                        lec.IdAnimal = animal.Id;
                    }
                }

                if (!idAnimal.HasValue)
                {
                    sinAnimal++;
                    lec.Observacion = (lec.Observacion ?? "") + " | No se pudo mapear Animal";
                    continue;
                }

                // 2) Determinar turno
                var turno = TurnoDesdeNumero(lec.NumeroOrdeno);
                var fechaDia = lec.FechaHoraLectura.Date;

                // 3) Buscar producción existente ese día/turno
                var prod = await _context.RegistroProduccionLeches.FirstOrDefaultAsync(p =>
                    p.idAnimal == idAnimal.Value &&
                    p.fechaOrdeno.HasValue &&
                    p.fechaOrdeno.Value.Date == fechaDia &&
                    p.turno == turno
                );

                if (prod == null)
                {
                    prod = new RegistroProduccionLeche
                    {
                        idAnimal = idAnimal.Value,
                        fechaRegistro = DateTime.Now,
                        turno = turno,
                        fechaOrdeno = lec.FechaHoraLectura,
                        fechaRetirada = lec.FechaHoraLectura, // no tienes fase real del ordeño del medidor → mínimo viable
                        pesoOrdeno = Math.Round((decimal)lec.PesoLecheKg, 2),
                        // destinos en 0 por defecto (quedan null si tu DB lo permite)
                        cantidadIndustria = 0,
                        cantidadTerneros = 0,
                        cantidadDescartada = 0,
                        cantidadVentaDirecta = 0,
                        tieneAntibiotico = false
                    };

                    _context.RegistroProduccionLeches.Add(prod);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Si ya existe, sumamos al peso (por si hay varias lecturas)
                    prod.pesoOrdeno = (prod.pesoOrdeno ?? 0) + Math.Round((decimal)lec.PesoLecheKg, 2);
                    _context.Update(prod);
                    await _context.SaveChangesAsync();
                }

                // 4) Marcar lectura procesada
                lec.Procesado = true;
                lec.IdRegistroProduccionLeche = prod.Id;
                ok++;
            }

            await _context.SaveChangesAsync();

            TempData["MedidorMsg"] = $"Procesadas: {ok}. Pendientes sin animal: {sinAnimal}.";
            return RedirectToAction(nameof(Index), new { procesado = false });
        }

        private static string TurnoDesdeNumero(byte n) =>
            n switch
            {
                1 => "MAÑANA",
                2 => "TARDE",
                3 => "NOCHE",
                _ => "OTRO"
            };
    }
}
