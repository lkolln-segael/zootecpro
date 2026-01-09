using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.Models;
using WebZootecPro.ViewModels.Dashboard;

namespace WebZootecPro.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ZootecContext _context;

        public HomeController(ILogger<HomeController> logger, ZootecContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // SUPERADMIN => no mostrar nada
            if (User.IsInRole("SUPERADMIN"))
                return RedirectToAction("Index", "Admin");


            // Usuario actual (Id viene en ClaimTypes.NameIdentifier según tu AccountController)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            var u = await _context.Usuarios
                .AsNoTracking()
                .Include(x => x.idHatoNavigation).ThenInclude(h => h.Establo)
                .Include(x => x.idEstabloNavigation).ThenInclude(e => e.Empresa)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (u == null)
                return Forbid();

            // Scope: si el usuario está amarrado a Hato, filtra por Hato. Si no, por Establo.
            int? hatoId = u.idHato;
            int? establoId = u.idEstablo ?? u.idHatoNavigation?.EstabloId;

            // Rango por defecto
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var desde = hoy.AddDays(-30);
            var hasta = hoy;

            // Tiempos para DateTime (Producción usa fechaRegistro DateTime)
            var hoyIni = DateTime.Today;
            var hoyFin = DateTime.Today.AddDays(1);

            var ini7 = DateTime.Today.AddDays(-6); // hoy + 6 días atrás = 7 días
            var fin7 = hoyFin;

            var dDesde = desde;
            var dHasta = hasta;

            // Query filtro animales (sin traer lista a memoria)
            IQueryable<Animal> animalesQ = _context.Animals.AsNoTracking();

            if (hatoId != null)
                animalesQ = animalesQ.Where(a => a.idHato == hatoId.Value);
            else if (establoId != null)
                animalesQ = animalesQ.Where(a => a.idHatoNavigation.EstabloId == establoId.Value);
            else
                animalesQ = animalesQ.Where(a => false);

            // --------------------------
            // 1) PRODUCCIÓN (HOY)
            // --------------------------
            var prodHoyLitros = await _context.RegistroProduccionLeches.AsNoTracking()
                .Where(r => r.fechaRegistro >= hoyIni && r.fechaRegistro < hoyFin)
                .Join(animalesQ,
                      r => r.idAnimal,
                      a => a.Id,
                      (r, a) => new
                      {
                          r.idAnimal,
                          Litros =
                              (r.cantidadIndustria ?? 0m) +
                              (r.cantidadTerneros ?? 0m) +
                              (r.cantidadDescartada ?? 0m) +
                              (r.cantidadVentaDirecta ?? 0m)
                      })
                .SumAsync(x => x.Litros);

            var vacasProdHoy = await _context.RegistroProduccionLeches.AsNoTracking()
                .Where(r => r.fechaRegistro >= hoyIni && r.fechaRegistro < hoyFin)
                .Join(animalesQ,
                      r => r.idAnimal,
                      a => a.Id,
                      (r, a) => r.idAnimal)
                .Distinct()
                .CountAsync();

            var promLitrosVacaHoy = vacasProdHoy == 0 ? 0m : Math.Round(prodHoyLitros / vacasProdHoy, 2);

            // --------------------------
            // 2) PRODUCCIÓN (7 DÍAS)
            // --------------------------
            var prod7Litros = await _context.RegistroProduccionLeches.AsNoTracking()
                .Where(r => r.fechaRegistro >= ini7 && r.fechaRegistro < fin7)
                .Join(animalesQ,
                      r => r.idAnimal,
                      a => a.Id,
                      (r, a) =>
                          (r.cantidadIndustria ?? 0m) +
                          (r.cantidadTerneros ?? 0m) +
                          (r.cantidadDescartada ?? 0m) +
                          (r.cantidadVentaDirecta ?? 0m))
                .SumAsync();

            var promDiario7 = Math.Round(prod7Litros / 7m, 2);

            // --------------------------
            // 3) REPRODUCCIÓN (30 DÍAS)
            // Servicios: Prenez.fechaInseminacion dentro del periodo
            // Concepciones: servicios del periodo con confirmación POSITIVA
            // --------------------------
            // (usamos el mismo criterio que tu ReportesController)
            var serviciosPeriodo = await _context.Prenezs.AsNoTracking()
                .Where(p => p.idMadreAnimal != null && p.fechaInseminacion != null)
                .Where(p => p.fechaInseminacion!.Value >= dDesde && p.fechaInseminacion!.Value <= dHasta)
                .Join(animalesQ,
                      p => p.idMadreAnimal!.Value,
                      a => a.Id,
                      (p, a) => p.Id)
                .CountAsync();

            var concepcionesPeriodo = await (
                from p in _context.Prenezs.AsNoTracking()
                where p.idMadreAnimal != null && p.fechaInseminacion != null
                let fi = p.fechaInseminacion!.Value
                where fi >= dDesde && fi <= dHasta
                join a in animalesQ on p.idMadreAnimal!.Value equals a.Id
                where _context.ConfirmacionPrenezs.Any(c =>
                    c.idRegistroReproduccion == p.idRegistroReproduccion &&
                    c.tipo == "POSITIVA")
                select p.idRegistroReproduccion
            ).Distinct().CountAsync();

            // Elegibles: en tu Reporte es una lógica más compleja (PVE, último parto, etc.).
            // Para dashboard, usamos una base simple: hembras activas = todas las del scope (si quieres, luego lo igualamos al reporte).
            // Si quieres EXACTO igual al reporte, se copia esa lógica al dashboard.
            var baseElegibles = await animalesQ.CountAsync();

            decimal pct(decimal num, decimal den) => den == 0 ? 0 : Math.Round((num / den) * 100m, 2);

            var tasaPrenez = pct(concepcionesPeriodo, baseElegibles);
            var tasaInsemin = pct(serviciosPeriodo, baseElegibles);

            // --------------------------
            // 4) ENFERMERÍA
            // Activos: fechaRecuperacion == null
            // Nuevos 7 días: fechaDiagnostico >= hoy-7
            // --------------------------
            var ini7dt = DateTime.Today.AddDays(-6);

            var enfermeriaActivos = await _context.Enfermedads.AsNoTracking()
                .Where(e => e.fechaRecuperacion == null)
                .Join(animalesQ,
                      e => e.idAnimal,
                      a => a.Id,
                      (e, a) => e.Id)
                .CountAsync();

            var enfermeriaNuevos7 = await _context.Enfermedads.AsNoTracking()
                .Where(e => e.fechaDiagnostico >= ini7dt && e.fechaDiagnostico < hoyFin)
                .Join(animalesQ,
                      e => e.idAnimal,
                      a => a.Id,
                      (e, a) => e.Id)
                .CountAsync();

            // --------------------------
            // 5) RTM (HOY)
            // Si usuario está en Hato => solo ese hato
            // Si está en Establo => todos los hatos de ese establo
            // --------------------------
            IQueryable<RtmEntrega> rtmQ = _context.RtmEntregas.AsNoTracking()
                .Where(r => r.fecha == hoy);

            if (hatoId != null)
            {
                rtmQ = rtmQ.Where(r => r.hatoId == hatoId.Value);
            }
            else if (establoId != null)
            {
                rtmQ = rtmQ.Join(_context.Hatos.AsNoTracking().Where(h => h.EstabloId == establoId.Value),
                                 r => r.hatoId,
                                 h => h.Id,
                                 (r, h) => r);
            }
            else
            {
                rtmQ = rtmQ.Where(r => false);
            }

            var entregasHoy = await rtmQ.CountAsync();
            var kgHoy = await rtmQ.SumAsync(r => (decimal?)r.kgTotal) ?? 0m;

            // --------------------------
            // 6) COSTOS (30 DÍAS)
            // MovimientoCosto tiene IdEstablo (ideal para filtrar).
            // --------------------------
            var costosDesde = hoy.AddDays(-30);

            IQueryable<MovimientoCosto> costosQ = _context.MovimientoCostos.AsNoTracking()
                .Where(c => c.Fecha >= costosDesde && c.Fecha <= hoy);

            if (establoId != null)
                costosQ = costosQ.Where(c => c.IdEstablo == establoId.Value);
            else
                costosQ = costosQ.Where(c => false);

            var costo30 = await costosQ.SumAsync(c => (decimal?)c.MontoTotal) ?? 0m;

            // Costo por litro aprox usando producción de últimos 30 (misma lógica sum)
            var ini30 = DateTime.Today.AddDays(-29);
            var fin30 = hoyFin;

            var prod30 = await _context.RegistroProduccionLeches.AsNoTracking()
                .Where(r => r.fechaRegistro >= ini30 && r.fechaRegistro < fin30)
                .Join(animalesQ,
                      r => r.idAnimal,
                      a => a.Id,
                      (r, a) =>
                          (r.cantidadIndustria ?? 0m) +
                          (r.cantidadTerneros ?? 0m) +
                          (r.cantidadDescartada ?? 0m) +
                          (r.cantidadVentaDirecta ?? 0m))
                .SumAsync();

            var costoPorLitro = prod30 <= 0 ? 0m : Math.Round(costo30 / prod30, 4);

            var vm = new DashboardViewModel
            {
                Desde = desde,
                Hasta = hasta,

                ProduccionHoyLitros = Math.Round(prodHoyLitros, 2),
                VacasProduciendoHoy = vacasProdHoy,
                PromedioLitrosPorVacaHoy = promLitrosVacaHoy,
                ProduccionUltimos7DiasLitros = Math.Round(prod7Litros, 2),
                PromedioDiario7Dias = promDiario7,

                ServiciosEnPeriodo = serviciosPeriodo,
                ConcepcionesEnPeriodo = concepcionesPeriodo,
                TasaPrenezPct = tasaPrenez,
                TasaInseminacionPct = tasaInsemin,

                CasosEnfermeriaActivos = enfermeriaActivos,
                CasosNuevos7Dias = enfermeriaNuevos7,

                EntregasRtmHoy = entregasHoy,
                KgRtmHoy = Math.Round(kgHoy, 2),

                CostoUltimos30Dias = Math.Round(costo30, 2),
                CostoPorLitroAprox = costoPorLitro
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
