using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Animales;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA,INSPECTOR,VETERINARIO")]
    public class AnimalesController : Controller
    {
        private readonly ZootecContext _context;
        private bool IsSuperAdmin => User.IsInRole("SUPERADMIN");
        private bool IsAdminEmpresa => User.IsInRole("ADMIN_EMPRESA");

        public AnimalesController(ZootecContext context)
        {
            _context = context;
        }

        // ----------------- HELPERS -----------------
        private async Task<IQueryable<Animal>> ScopeAnimalesAsync(IQueryable<Animal> q)
        {
            if (IsSuperAdmin) return q;

            var userId = GetCurrentUserId();
            if (userId == null) return q.Where(_ => false);

            if (IsAdminEmpresa)
            {
                // dueño: animales cuya empresa pertenece a este admin
                return q.Where(a =>
                    a.idHatoNavigation.Establo.Empresa.usuarioID == userId.Value
                 || a.idHatoNavigation.Establo.Empresa.Colaboradors.Any(c => c.idUsuario == userId.Value)
                );

            }

            // roles con establo/hato asignado
            var u = await GetCurrentUserAsync();
            if (u?.idHato != null) return q.Where(a => a.idHato == u.idHato.Value);
            if (u?.idEstablo != null) return q.Where(a => a.idHatoNavigation.EstabloId == u.idEstablo.Value);

            return q.Where(_ => false);
        }

        private async Task<IQueryable<Hato>> ScopeHatosAsync(IQueryable<Hato> q)
        {
            if (IsSuperAdmin) return q;

            var userId = GetCurrentUserId();
            if (userId == null) return q.Where(_ => false);

            if (IsAdminEmpresa)
            {
                return q.Where(h =>
                    h.Establo.Empresa.usuarioID == userId.Value
                    || h.Establo.Empresa.Colaboradors.Any(c => c.idUsuario == userId.Value)
                );
            }

            var u = await _context.Usuarios
                .AsNoTracking()
                .Where(x => x.Id == userId.Value)
                .Select(x => new { x.idHato, x.idEstablo })
                .FirstOrDefaultAsync();

            if (u == null) return q.Where(_ => false);
            if (u.idHato != null) return q.Where(h => h.Id == u.idHato.Value);
            if (u.idEstablo != null) return q.Where(h => h.EstabloId == u.idEstablo.Value);

            return q.Where(_ => false);
        }

        private async Task<int?> GetEmpresaIdPorHatoAsync(int hatoId)
        {
            return await _context.Hatos
                .AsNoTracking()
                .Where(h => h.Id == hatoId)
                .Select(h => (int?)h.Establo.EmpresaId)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> GetEstabloIdPorHatoAsync(int hatoId)
        {
            return await _context.Hatos
                .AsNoTracking()
                .Where(h => h.Id == hatoId)
                .Select(h => (int?)h.EstabloId)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> AnimalPerteneceAEstabloAsync(int animalId, int establoId)
        {
            var scope = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
            return await scope.AnyAsync(a => a.Id == animalId && a.idHatoNavigation.EstabloId == establoId);
        }


        private async Task<bool> AnimalEsVisibleAsync(int animalId)
        {
            var q = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
            return await q.AnyAsync(a => a.Id == animalId);
        }

        private async Task CargarCombosAsync(Animal? animal = null, int? excludeAnimalId = null)
        {
            // =========================
            // 1) HATOS (multiempresa real)
            // =========================
            List<Hato> hatos;

            if (IsSuperAdmin)
            {
                hatos = await _context.Hatos.AsNoTracking()
                    .OrderBy(h => h.nombre)
                    .ToListAsync();
            }
            else
            {
                var hatosQ = await ScopeHatosAsync(_context.Hatos.AsNoTracking());
                hatos = await hatosQ
                    .OrderBy(h => h.nombre)
                    .ToListAsync();
            }

            // Default: si no hay hato seleccionado, ponemos el primero visible (evita combos vacíos)
            if (animal != null && animal.idHato <= 0 && hatos.Count > 0)
                animal.idHato = hatos[0].Id;

            var slHatos = new SelectList(hatos, "Id", "nombre", animal?.idHato);
            ViewBag.IdHato = slHatos;
            ViewData["idHato"] = slHatos; // compatibilidad con vistas scaffold

            // =========================
            // 2) MAESTROS
            // =========================
            var slEstado = new SelectList(
                await _context.EstadoAnimals.AsNoTracking().OrderBy(e => e.nombre).ToListAsync(),
                "Id", "nombre", animal?.estadoId
            );
            ViewBag.EstadoId = slEstado;
            ViewData["estadoId"] = slEstado;

            var slProposito = new SelectList(
                await _context.PropositoAnimals.AsNoTracking().OrderBy(p => p.nombre).ToListAsync(),
                "Id", "nombre", animal?.propositoId
            );
            ViewBag.PropositoId = slProposito;
            ViewData["propositoId"] = slProposito;

            var slProcedencia = new SelectList(
                await _context.ProcedenciaAnimals.AsNoTracking().OrderBy(p => p.nombre).ToListAsync(),
                "Id", "nombre", animal?.procedenciaId
            );
            ViewBag.ProcedenciaId = slProcedencia;
            ViewData["procedenciaId"] = slProcedencia;

            var slRaza = new SelectList(
                await _context.Razas.AsNoTracking().OrderBy(r => r.nombre).ToListAsync(),
                "Id", "nombre", animal?.idRaza
            );
            ViewBag.IdRaza = slRaza;
            ViewData["idRaza"] = slRaza;

            var slEstadoProd = new SelectList(
                await _context.EstadoProductivos.AsNoTracking().OrderBy(e => e.nombre).ToListAsync(),
                "Id", "nombre", animal?.estadoProductivoId
            );
            ViewBag.EstadoProductivoId = slEstadoProd;
            ViewData["estadoProductivoId"] = slEstadoProd;

            var slCategoria = new SelectList(
                await _context.CategoriaAnimals.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(),
                "IdCategoriaAnimal", "Nombre", animal?.IdCategoriaAnimal
            );
            ViewBag.IdCategoriaAnimal = slCategoria;
            ViewData["IdCategoriaAnimal"] = slCategoria;

            // =========================
            // 3) PADRE / MADRE (filtrado por establo del hato seleccionado)
            // =========================
            int? establoId = null;

            if (animal?.idHato > 0)
            {
                if (IsSuperAdmin)
                {
                    establoId = await _context.Hatos.AsNoTracking()
                        .Where(h => h.Id == animal.idHato)
                        .Select(h => (int?)h.EstabloId)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    var hatosQ = await ScopeHatosAsync(_context.Hatos.AsNoTracking());
                    establoId = await hatosQ
                        .Where(h => h.Id == animal.idHato)
                        .Select(h => (int?)h.EstabloId)
                        .FirstOrDefaultAsync();
                }
            }


            var animalesScope = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

            var padresQ = animalesScope.Where(a =>
                a.sexo == "MACHO" && (excludeAnimalId == null || a.Id != excludeAnimalId));

            var madresQ = animalesScope.Where(a =>
                a.sexo == "HEMBRA" && (excludeAnimalId == null || a.Id != excludeAnimalId));

            // Si no hay hato seleccionado, no mostramos padres/madres para evitar cruces multiempresa
            if (establoId != null)
            {
                padresQ = padresQ.Where(a => a.idHatoNavigation.EstabloId == establoId.Value);
                madresQ = madresQ.Where(a => a.idHatoNavigation.EstabloId == establoId.Value);
            }
            else
            {
                padresQ = padresQ.Where(_ => false);
                madresQ = madresQ.Where(_ => false);
            }

            var padres = await padresQ
                .OrderBy(a => a.codigo)
                .Select(a => new { a.Id, Label = (a.codigo ?? "-") + " - " + (a.nombre ?? "-") })
                .ToListAsync();

            var madres = await madresQ
                .OrderBy(a => a.codigo)
                .Select(a => new { a.Id, Label = (a.codigo ?? "-") + " - " + (a.nombre ?? "-") })
                .ToListAsync();

            var slPadre = new SelectList(padres, "Id", "Label", animal?.idPadre);
            var slMadre = new SelectList(madres, "Id", "Label", animal?.idMadre);

            ViewBag.IdPadre = slPadre;
            ViewData["idPadre"] = slPadre;

            ViewBag.IdMadre = slMadre;
            ViewData["idMadre"] = slMadre;
        }



        private void LimpiarValidacionNavegaciones()
        {
            ModelState.Remove(nameof(Animal.idHatoNavigation));
            ModelState.Remove(nameof(Animal.idPadreNavigation));
            ModelState.Remove(nameof(Animal.idMadreNavigation));
            ModelState.Remove(nameof(Animal.idUltimoCrecimientoNavigation));

            // NUEVAS navegaciones
            ModelState.Remove(nameof(Animal.estado));
            ModelState.Remove(nameof(Animal.proposito));
            ModelState.Remove(nameof(Animal.procedencia));
            ModelState.Remove(nameof(Animal.idRazaNavigation));
            ModelState.Remove(nameof(Animal.estadoProductivo));

            // colecciones
            ModelState.Remove(nameof(Animal.Alimentacions));
            ModelState.Remove(nameof(Animal.DesarrolloCrecimientos));
            ModelState.Remove(nameof(Animal.Enfermedads));
            ModelState.Remove(nameof(Animal.InverseidMadreNavigation));
            ModelState.Remove(nameof(Animal.InverseidPadreNavigation));
            ModelState.Remove(nameof(Animal.PrenezidMadreAnimalNavigations));
            ModelState.Remove(nameof(Animal.PrenezidPadreAnimalNavigations));
            ModelState.Remove(nameof(Animal.RegistroIngresos));
            ModelState.Remove(nameof(Animal.RegistroNacimientos));
            ModelState.Remove(nameof(Animal.RegistroProduccionLeches));
            ModelState.Remove(nameof(Animal.RegistroReproduccions));
            ModelState.Remove(nameof(Animal.RegistroSalida));
            ModelState.Remove(nameof(Animal.TipoAlimentos));
        }

        private int? GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : null;
        }

        private async Task<Usuario?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;

            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.idHatoNavigation).ThenInclude(h => h.Establo)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);
        }

        private async Task<bool> HatoEsVisibleAsync(int hatoId)
        {
            if (IsSuperAdmin) return true;

            var userId = GetCurrentUserId();
            if (userId == null) return false;

            // ADMIN_EMPRESA: hatos cuyo establo pertenece a una empresa del usuario (Empresa.usuarioID)
            if (IsAdminEmpresa)
            {
                return await _context.Hatos.AnyAsync(h =>
                    h.Id == hatoId &&
                    (
                        h.Establo.Empresa.usuarioID == userId.Value
                        || h.Establo.Empresa.Colaboradors.Any(c => c.idUsuario == userId.Value)
                    )
                );
            }


            // Otros roles: si está amarrado a Hato => solo ese Hato.
            // Si está amarrado a Establo => cualquier Hato de ese Establo.
            var u = await _context.Usuarios
                .AsNoTracking()
                .Where(x => x.Id == userId.Value)
                .Select(x => new { x.idHato, x.idEstablo })
                .FirstOrDefaultAsync();

            if (u == null) return false;
            if (u.idHato != null) return u.idHato.Value == hatoId;
            if (u.idEstablo != null) return await _context.Hatos.AnyAsync(h => h.Id == hatoId && h.EstabloId == u.idEstablo.Value);

            return false;
        }

        private async Task<string?> ValidarLimiteAnimalesAsync(int hatoId)
        {
            // Seguridad: que no “postee” un hato que no le corresponde
            if (!await HatoEsVisibleAsync(hatoId))
                return "Hato inválido para este usuario.";

            // Empresa del hato
            var empresaId = await _context.Hatos
                .Where(h => h.Id == hatoId)
                .Select(h => (int?)h.Establo.EmpresaId)
                .FirstOrDefaultAsync();

            if (empresaId == null)
                return "No se pudo determinar la empresa del hato.";

            // PlanId + fallback capacidadMaxima
            var emp = await _context.Empresas
                .Where(e => e.Id == empresaId.Value)
                .Select(e => new { e.PlanId, e.capacidadMaxima })
                .FirstOrDefaultAsync();

            if (emp == null)
                return "Empresa no encontrada.";

            int? maxAnimales = null;

            // SI hay PlanId -> usar PlanLicencia.MaxAnimales
            if (emp.PlanId != null)
            {
                // Ajusta el nombre del DbSet según tu scaffold: PlanLicencia / PlanLicencias / etc.
                maxAnimales = await _context.PlanLicencia
                    .Where(p => p.Id == emp.PlanId.Value && p.Activo)
                    .Select(p => p.MaxAnimales)
                    .FirstOrDefaultAsync();
            }
            else
            {
                // fallback antiguo (si no asignaste plan aún)
                maxAnimales = emp.capacidadMaxima;
            }

            // NULL = ilimitado
            if (maxAnimales == null) return null;

            // Contamos animales ACTIVOS de esa empresa (cámbialo a “todos” si quieres)
            var activoId = await _context.EstadoAnimals
                .Where(x => x.nombre == "ACTIVO")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            var q = from a in _context.Animals
                    join h in _context.Hatos on a.idHato equals h.Id
                    join est in _context.Establos on h.EstabloId equals est.Id
                    where est.EmpresaId == empresaId.Value
                    select a;

            if (activoId != null)
                q = q.Where(a => a.estadoId == activoId.Value);

            var actuales = await q.CountAsync();

            // Si ya está en el máximo, no se puede agregar 1 más
            if (actuales >= maxAnimales.Value)
                return $"Límite alcanzado: tu plan permite {maxAnimales.Value} animales activos. Actualmente tienes {actuales}.";

            return null;
        }

        private async Task<Empresa?> GetEmpresaAsync()
        {
            var usuarioId = GetCurrentUserId();
            var usuario = await GetCurrentUserAsync();
            return await _context.Empresas.FirstOrDefaultAsync(e => e.usuarioID == usuarioId
                || e.Colaboradors.Select(e => e.idUsuario).Contains(usuarioId.Value));
        }

        public async Task<IActionResult> Index()
        {
            // 0) Animales visibles (según rol) -> solo hembras
            var animalesQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

            var animales = await animalesQ
                .Where(a => a.sexo != null && a.sexo.Trim().ToUpper() == "HEMBRA")
                .Include(a => a.idHatoNavigation)
                .Include(a => a.idPadreNavigation)
                    .ThenInclude(p => p!.idPadreNavigation)
                .Include(a => a.idMadreNavigation)
                    .ThenInclude(m => m!.idPadreNavigation)
                .OrderBy(a => a.codigo)
                .ToListAsync();

            var animalIds = animales.Select(a => a.Id).ToList();

            // 1) Partos por animal (solo animales visibles)
            var partosPorAnimal = await _context.Partos
                .AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => rr.idAnimal)
                .Where(idAnimal => animalIds.Contains(idAnimal))
                .GroupBy(idAnimal => idAnimal)
                .Select(g => new { AnimalId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.AnimalId, x => x.Total);

            // 2) Hatos (desde los animales ya cargados)
            var hatos = animales
                .Select(a => new { a.idHato, Nombre = a.idHatoNavigation.nombre })
                .GroupBy(x => x.idHato)
                .ToDictionary(g => g.Key, g => g.First().Nombre);

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            string FormatoAnimal(WebZootecPro.Data.Animal? a)
                => a == null ? "-" : $"{(a.codigo ?? "-")} - {(a.nombre ?? "-")}";

            string CalcularEdadTexto(DateOnly? nacimiento)
            {
                if (nacimiento == null) return "-";

                var n = nacimiento.Value;
                int years = hoy.Year - n.Year;
                int months = hoy.Month - n.Month;

                if (hoy.Day < n.Day) months--;

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                if (years < 0) return "-";

                return $"{years}a {months}m";
            }

            var model = animales.Select(a =>
            {
                var padre = a.idPadreNavigation;
                var madre = a.idMadreNavigation;

                var abueloPaterno = padre?.idPadreNavigation;
                var abueloMaterno = madre?.idPadreNavigation;

                return new WebZootecPro.ViewModels.Animales.AnimalListadoVm
                {
                    Id = a.Id,
                    Codigo = a.codigo,
                    Nombre = a.nombre,
                    Sexo = a.sexo,
                    FechaNacimiento = a.fechaNacimiento,
                    EdadTexto = CalcularEdadTexto(a.fechaNacimiento),

                    Hato = hatos.TryGetValue(a.idHato, out var h) ? h : "-",

                    Padre = FormatoAnimal(padre),
                    Madre = FormatoAnimal(madre),
                    AbueloPaterno = FormatoAnimal(abueloPaterno),
                    AbueloMaterno = FormatoAnimal(abueloMaterno),

                    NumeroPartos = partosPorAnimal.TryGetValue(a.Id, out var c) ? c : 0
                };
            }).ToList();

            return View(model);
        }



        // ----------------- DETAILS -----------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var animalQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

            var animal = await animalQ
                .Include(a => a.idHatoNavigation)
                .Include(a => a.idRazaNavigation)
                .Include(a => a.procedencia)
                .Include(a => a.proposito)
                .Include(a => a.estado)
                .Include(a => a.estadoProductivo)

                .Include(a => a.idPadreNavigation)
                    .ThenInclude(p => p!.idPadreNavigation)
                .Include(a => a.idPadreNavigation)
                    .ThenInclude(p => p!.idMadreNavigation)

                .Include(a => a.idMadreNavigation)
                    .ThenInclude(m => m!.idPadreNavigation)
                .Include(a => a.idMadreNavigation)
                    .ThenInclude(m => m!.idMadreNavigation)

                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (animal == null) return NotFound();

            // Abuelos (derivados)
            var abueloPaterno = animal.idPadreNavigation?.idPadreNavigation;
            var abuelaPaterna = animal.idPadreNavigation?.idMadreNavigation;
            var abueloMaterno = animal.idMadreNavigation?.idPadreNavigation;
            var abuelaMaterna = animal.idMadreNavigation?.idMadreNavigation;

            // Campañas del establo del animal
            var establoId = animal.idHatoNavigation.EstabloId;

            var campanias = await _context.CampaniaLecheras
                .AsNoTracking()
                .Where(c => c.EstabloId == establoId && c.activa)
                .OrderByDescending(c => c.fechaInicio)
                .ToListAsync();

            // Traemos todas las producciones del animal una sola vez (para calcular en memoria)
            var regs = await _context.RegistroProduccionLeches
                .AsNoTracking()
                .Where(r => r.idAnimal == animal.Id)
                .Select(r => new
                {
                    r.fechaRegistro,
                    Producido = (decimal)(r.pesoOrdeno ?? 0),
                    Industria = (decimal)(r.cantidadIndustria ?? 0),
                    Terneros = (decimal)(r.cantidadTerneros ?? 0),
                    Descartada = (decimal)(r.cantidadDescartada ?? 0),
                    Venta = (decimal)(r.cantidadVentaDirecta ?? 0)
                })
                .ToListAsync();

            var items = new List<ProduccionCampaniaItemVm>();

            foreach (var c in campanias)
            {
                var ini = c.fechaInicio.ToDateTime(TimeOnly.MinValue);
                var fin = c.fechaFin.ToDateTime(TimeOnly.MaxValue);

                var q = regs.Where(x => x.fechaRegistro >= ini && x.fechaRegistro <= fin);

                items.Add(new ProduccionCampaniaItemVm
                {
                    IdCampania = c.Id,
                    Nombre = c.nombre,
                    FechaInicio = c.fechaInicio,
                    FechaFin = c.fechaFin,
                    Producido = q.Sum(x => x.Producido),
                    Industria = q.Sum(x => x.Industria),
                    Terneros = q.Sum(x => x.Terneros),
                    Descartada = q.Sum(x => x.Descartada),
                    VentaDirecta = q.Sum(x => x.Venta)
                });
            }

            var vm = new AnimalDetallesViewModel
            {
                Animal = animal,
                AbueloPaterno = abueloPaterno,
                AbuelaPaterna = abuelaPaterna,
                AbueloMaterno = abueloMaterno,
                AbuelaMaterna = abuelaMaterna,
                ProduccionPorCampania = items
            };

            return View(vm);
        }


        // ----------------- CREATE -----------------
        [HttpGet]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA")]
        public async Task<IActionResult> Create()
        {

            var animal = new Animal
            {
                nacimientoEstimado = false,
                sexo = "HEMBRA" // ✅ forzado
            };

            // defaults recomendados
            animal.estadoId = await _context.EstadoAnimals
                .Where(x => x.nombre == "ACTIVO")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            animal.procedenciaId = await _context.ProcedenciaAnimals
                .Where(x => x.nombre == "DESCONOCIDA")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            animal.estadoProductivoId = await _context.EstadoProductivos
    .Where(x => x.nombre == "DESCONOCIDA")
    .Select(x => (int?)x.Id)
    .FirstOrDefaultAsync();


            await CargarCombosAsync(animal);
            return View(animal);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA")]
        public async Task<IActionResult> Create(Animal animal)
        {
            LimpiarValidacionNavegaciones();

            animal.sexo = "HEMBRA";
            ModelState.Remove(nameof(Animal.sexo));

            // ---------------- Defaults ----------------
            animal.estadoId ??= await _context.EstadoAnimals
                .Where(x => x.nombre == "ACTIVO")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            animal.procedenciaId ??= await _context.ProcedenciaAnimals
                .Where(x => x.nombre == "DESCONOCIDA")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            animal.estadoProductivoId = await _context.EstadoProductivos
    .Where(x => x.nombre == "DESCONOCIDA")
    .Select(x => (int?)x.Id)
    .FirstOrDefaultAsync();


            // Limpia required (porque ya setea defaults)
            ModelState.Remove(nameof(Animal.estadoId));
            ModelState.Remove(nameof(Animal.procedenciaId));

            // Si por alguna razón no existen los maestros:
            if (animal.estadoId == null)
                ModelState.AddModelError(nameof(Animal.estadoId), "No existe el estado ACTIVO en EstadoAnimal.");
            if (animal.procedenciaId == null)
                ModelState.AddModelError(nameof(Animal.procedenciaId), "No existe la procedencia DESCONOCIDA en ProcedenciaAnimal.");

            animal.nombre = (animal.nombre ?? "").Trim();
            animal.codigo = (animal.codigo ?? "").Trim();
            animal.arete = (animal.arete ?? "").Trim().ToUpper();


            // --- ARETE obligatorio + único ---
            if (string.IsNullOrWhiteSpace(animal.arete))
                ModelState.AddModelError(nameof(animal.arete), "Ingrese arete.");

            if (!string.IsNullOrWhiteSpace(animal.arete))
            {
                // hato debe ser visible (seguridad)
                if (!await HatoEsVisibleAsync(animal.idHato))
                {
                    ModelState.AddModelError(nameof(animal.idHato), "Hato inválido para este usuario.");
                }
                else
                {
                    var empresaId = await GetEmpresaIdPorHatoAsync(animal.idHato);
                    if (empresaId == null)
                    {
                        ModelState.AddModelError(nameof(animal.idHato), "No se pudo determinar la empresa del hato.");
                    }
                    else
                    {
                        var existeArete = await _context.Animals
                            .Where(a => a.arete == animal.arete)
                            .Join(_context.Hatos, a => a.idHato, h => h.Id, (a, h) => new { a, h })
                            .AnyAsync(x => x.h.Establo.EmpresaId == empresaId.Value);

                        if (existeArete)
                            ModelState.AddModelError(nameof(animal.arete), "El arete ya existe en esta empresa, ingrese otro.");
                    }
                }
            }

            // Validar Hato SIEMPRE (aunque arete esté vacío)
            if (!await HatoEsVisibleAsync(animal.idHato))
                ModelState.AddModelError(nameof(animal.idHato), "Hato inválido para este usuario.");

            // Validar que Padre/Madre sean del mismo establo del hato seleccionado
            var establoIdSel = await GetEstabloIdPorHatoAsync(animal.idHato);
            if (establoIdSel == null)
            {
                ModelState.AddModelError(nameof(animal.idHato), "No se pudo determinar el establo del hato.");
            }
            else
            {
                if (animal.idPadre != null && !await AnimalPerteneceAEstabloAsync(animal.idPadre.Value, establoIdSel.Value))
                    ModelState.AddModelError(nameof(animal.idPadre), "El padre no pertenece al establo del hato seleccionado.");

                if (animal.idMadre != null && !await AnimalPerteneceAEstabloAsync(animal.idMadre.Value, establoIdSel.Value))
                    ModelState.AddModelError(nameof(animal.idMadre), "La madre no pertenece al establo del hato seleccionado.");
            }

            // ---------------- Validación normal ----------------
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                return View(animal);
            }

            // Usuario actual (para RegistroIngreso)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = int.TryParse(userIdStr, out var u) ? u : null;

            // =========================================================
            // TRANSACCIÓN ÚNICA y FUERTE (evita doble alta concurrente)
            // =========================================================
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // Revalidar licencia DENTRO de la transacción (anti-concurrencia)
                var errorLicencia = await ValidarLimiteAnimalesAsync(animal.idHato);
                if (errorLicencia != null)
                {
                    ModelState.AddModelError(string.Empty, errorLicencia);
                    await tx.RollbackAsync();

                    await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                    return View(animal);
                }

                // Revalidar ARETE DENTRO de la transacción (anti-concurrencia)
                var empresaIdTx = await GetEmpresaIdPorHatoAsync(animal.idHato);
                if (empresaIdTx == null)
                {
                    ModelState.AddModelError(nameof(animal.idHato), "No se pudo determinar la empresa del hato.");
                    await tx.RollbackAsync();
                    await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                    return View(animal);
                }

                var existeAreteTx = await _context.Animals
                    .Where(a => a.arete == animal.arete)
                    .Join(_context.Hatos, a => a.idHato, h => h.Id, (a, h) => new { a, h })
                    .AnyAsync(x => x.h.Establo.EmpresaId == empresaIdTx.Value);

                if (existeAreteTx)
                {
                    ModelState.AddModelError(nameof(animal.arete), "El arete ya existe en esta empresa, ingrese otro.");
                    await tx.RollbackAsync();
                    await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                    return View(animal);
                }

                // 1) Guardar animal para obtener Id
                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();

                // 2) Registrar ingreso automático
                var codigoIngreso = $"ING-{DateTime.Now:yyyyMMddHHmmss}-{animal.Id}";

                _context.RegistroIngresos.Add(new RegistroIngreso
                {
                    codigoIngreso = codigoIngreso,
                    tipoIngreso = "ALTA",
                    idAnimal = animal.Id,
                    fechaIngreso = DateOnly.FromDateTime(DateTime.Today),
                    idHato = animal.idHato,
                    usuarioId = userId,
                    origen = null,
                    observacion = "Alta automática al crear el animal"
                });

                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    $"Error al guardar animal: {ex.InnerException?.Message ?? ex.Message}");

                await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                return View(animal);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");

                await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                return View(animal);
            }
        }


        // ----------------- EDIT -----------------
        [HttpGet]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA")]
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null) return NotFound();

            var animalQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
            var animal = await animalQ.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null) return NotFound();

            await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
            return View(animal);
        }

        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
    "Id,nombre,sexo,codigo,arete,IdentificadorElectronico,OtroIdentificador,color,fechaNacimiento,observaciones," +
    "idHato,idPadre,idMadre,estadoId,propositoId,idRaza,procedenciaId,nacimientoEstimado,estadoProductivoId,IdCategoriaAnimal"
)] Animal model)
        {
            LimpiarValidacionNavegaciones();
            // 0) Seguridad base
            if (id != model.Id) return NotFound();
            if (!await AnimalEsVisibleAsync(id)) return Forbid();
            if (!await HatoEsVisibleAsync(model.idHato)) return Forbid();

            // 1) Validar establo del hato
            var establoIdSel = await GetEstabloIdPorHatoAsync(model.idHato);
            if (establoIdSel == null)
            {
                ModelState.AddModelError(nameof(model.idHato), "No se pudo determinar el establo del hato.");
                await CargarCombosAsync(model, excludeAnimalId: model.Id);
                return View(model);
            }

            // 2) Validar padre/madre: visibles + del mismo establo
            if (model.idPadre != null)
            {
                if (!await AnimalEsVisibleAsync(model.idPadre.Value)) return Forbid();

                if (!await AnimalPerteneceAEstabloAsync(model.idPadre.Value, establoIdSel.Value))
                {
                    ModelState.AddModelError(nameof(model.idPadre), "El padre no pertenece al establo del hato seleccionado.");
                    await CargarCombosAsync(model, excludeAnimalId: model.Id);
                    return View(model);
                }
            }

            if (model.idMadre != null)
            {
                if (!await AnimalEsVisibleAsync(model.idMadre.Value)) return Forbid();

                if (!await AnimalPerteneceAEstabloAsync(model.idMadre.Value, establoIdSel.Value))
                {
                    ModelState.AddModelError(nameof(model.idMadre), "La madre no pertenece al establo del hato seleccionado.");
                    await CargarCombosAsync(model, excludeAnimalId: model.Id);
                    return View(model);
                }
            }

            // Scaffold: evita validación por navegación

            // 3) Arete obligatorio
            model.arete = (model.arete ?? "").Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(model.arete))
                ModelState.AddModelError(nameof(model.arete), "Ingrese arete.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(model, excludeAnimalId: model.Id);
                return View(model);
            }

            // =========================================================
            // 4) TRANSACCIÓN SERIALIZABLE (anti-concurrencia de arete)
            // =========================================================
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // Traer el registro real dentro del TX
                var animalDb = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
                if (animalDb == null)
                {
                    await tx.RollbackAsync();
                    return NotFound();
                }

                // Empresa del hato (dentro del TX)
                var empresaIdEdit = await GetEmpresaIdPorHatoAsync(model.idHato);
                if (empresaIdEdit == null)
                {
                    ModelState.AddModelError(nameof(model.idHato), "No se pudo determinar la empresa del hato.");
                    await tx.RollbackAsync();
                    await CargarCombosAsync(model, excludeAnimalId: model.Id);
                    return View(model);
                }

                // Revalidar ARETE único dentro del TX
                var existeAreteTx = await _context.Animals
                    .Where(a => a.Id != id && a.arete == model.arete)
                    .Join(_context.Hatos, a => a.idHato, h => h.Id, (a, h) => new { a, h })
                    .AnyAsync(x => x.h.Establo.EmpresaId == empresaIdEdit.Value);

                if (existeAreteTx)
                {
                    ModelState.AddModelError(nameof(model.arete), "El arete ya existe en esta empresa, ingrese otro.");
                    await tx.RollbackAsync();
                    await CargarCombosAsync(model, excludeAnimalId: model.Id);
                    return View(model);
                }

                // 5) Actualizar campos
                animalDb.nombre = model.nombre;
                animalDb.sexo = model.sexo;
                animalDb.codigo = model.codigo;
                animalDb.arete = model.arete;
                animalDb.IdentificadorElectronico = model.IdentificadorElectronico;
                animalDb.OtroIdentificador = model.OtroIdentificador;
                animalDb.color = model.color;
                animalDb.fechaNacimiento = model.fechaNacimiento;
                animalDb.observaciones = model.observaciones;
                animalDb.idHato = model.idHato;
                animalDb.idPadre = model.idPadre;
                animalDb.idMadre = model.idMadre;
                animalDb.estadoId = model.estadoId;
                animalDb.propositoId = model.propositoId;
                animalDb.idRaza = model.idRaza;
                animalDb.procedenciaId = model.procedenciaId;
                animalDb.nacimientoEstimado = model.nacimientoEstimado;
                animalDb.IdCategoriaAnimal = model.IdCategoriaAnimal;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "El registro fue modificado por otro usuario. Vuelva a intentar.");
                await CargarCombosAsync(model, excludeAnimalId: model.Id);
                return View(model);
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
                await CargarCombosAsync(model, excludeAnimalId: model.Id);
                return View(model);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                await CargarCombosAsync(model, excludeAnimalId: model.Id);
                return View(model);
            }
        }
        // ----------------- DELETE -----------------
        [HttpGet]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var animalQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
            var animal = await animalQ.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null) return NotFound();

            return View(animal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animalQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
            var ok = await animalQ.AnyAsync(a => a.Id == id);
            if (!ok) return Forbid();

            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                _context.Animals.Remove(animal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
            var animal = await (await ScopeAnimalesAsync(_context.Animals.AsNoTracking()))
    .FirstOrDefaultAsync(a => a.Id == id);

            if (animal == null) return NotFound();

            var items = new List<HistorialMovimientoVm>();

            // INGRESOS (sin fecha en tu tabla, se ordena por Id)
            var ingresosDb = await _context.RegistroIngresos.AsNoTracking()
      .Where(x => x.idAnimal == id)
      .Select(x => new { x.Id, x.fechaIngreso, x.tipoIngreso, x.codigoIngreso })
      .ToListAsync();

            items.AddRange(ingresosDb.Select(x => new HistorialMovimientoVm
            {
                Fecha = x.fechaIngreso.ToDateTime(TimeOnly.MinValue),  // <-- AQUÍ
                Orden = x.Id,
                Fuente = "Ingreso",
                Evento = x.tipoIngreso,
                Detalle = $"Código: {x.codigoIngreso}"
            }));


            // SALIDAS (sin fecha en tu tabla, se ordena por Id)
            var salidasDb = await _context.RegistroSalida.AsNoTracking()
      .Where(x => x.idAnimal == id)
      .Select(x => new { x.Id, x.fechaSalida, x.tipoSalida, x.nombre })
      .ToListAsync();

            items.AddRange(salidasDb.Select(x => new HistorialMovimientoVm
            {
                Fecha = x.fechaSalida.ToDateTime(TimeOnly.MinValue),   // <-- AQUÍ
                Orden = x.Id,
                Fuente = "Salida",
                Evento = x.tipoSalida,
                Detalle = $"Nombre/Destino: {x.nombre}"
            }));


            // NACIMIENTOS
            var nac = await _context.RegistroNacimientos.AsNoTracking()
                .Where(x => x.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.fecha.ToDateTime(TimeOnly.MinValue),
                    Orden = x.Id,
                    Fuente = "Producción",
                    Evento = "Nacimiento",
                    Detalle = $"Peso: {(x.pesoNacimiento.HasValue ? x.pesoNacimiento.Value.ToString("0.##") : "-")}"
                })
                .ToListAsync();
            items.AddRange(nac);

            // PRODUCCIÓN LECHE
            var leche = await _context.RegistroProduccionLeches.AsNoTracking()
                .Where(x => x.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.fechaRegistro,
                    Orden = x.Id,
                    Fuente = "Producción",
                    Evento = "Producción leche",
                    Detalle = $"Peso ordeño: {(x.pesoOrdeno.HasValue ? x.pesoOrdeno.Value.ToString("0.##") : "-")}"
                })
                .ToListAsync();
            items.AddRange(leche);

            // CRECIMIENTO
            var cre = await _context.DesarrolloCrecimientos.AsNoTracking()
                .Where(x => x.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.fechaRegistro,
                    Orden = x.Id,
                    Fuente = "Producción",
                    Evento = "Crecimiento",
                    Detalle = $"Peso: {(x.pesoActual.HasValue ? x.pesoActual.Value.ToString("0.##") : "-")} / Talla: {(x.tamano.HasValue ? x.tamano.Value.ToString("0.##") : "-")}"
                })
                .ToListAsync();
            items.AddRange(cre);

            // ALIMENTACIÓN
            var ali = await _context.Alimentacions.AsNoTracking()
                .Where(x => x.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.fecha.ToDateTime(TimeOnly.MinValue),
                    Orden = x.Id,
                    Fuente = "Producción",
                    Evento = "Alimentación",
                    Detalle = $"Cantidad: {(x.cantidad.HasValue ? x.cantidad.Value.ToString("0.##") : "-")} / Estado: {(x.estado ?? "-")}"
                })
                .ToListAsync();
            items.AddRange(ali);

            // SANIDAD: ENFERMEDAD
            var enf = await _context.Enfermedads.AsNoTracking()
                .Where(x => x.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.fechaDiagnostico,
                    Orden = x.Id,
                    Fuente = "Sanidad",
                    Evento = "Enfermedad",
                    Detalle = $"{x.idTipoEnfermedadNavigation.nombre} (Vet: {x.idVeterinarioNavigation.nombre})"
                })
                .ToListAsync();
            items.AddRange(enf);

            // SANIDAD: TRATAMIENTOS (join por enfermedad -> animal)
            var trat = await _context.Tratamientos.AsNoTracking()
                .Join(_context.Enfermedads.AsNoTracking(),
                    t => t.idEnfermedad,
                    e => e.Id,
                    (t, e) => new { t, e })
                .Where(x => x.e.idAnimal == id)
                .Join(_context.TipoTratamientos.AsNoTracking(),
                    x => x.t.idTipoTratamiento,
                    tt => tt.Id,
                    (x, tt) => new HistorialMovimientoVm
                    {
                        Fecha = x.t.fechaInicio,
                        Orden = x.t.Id,
                        Fuente = "Sanidad",
                        Evento = "Tratamiento",
                        Detalle = $"{tt.nombre} / Obs: {(x.t.observaciones ?? "-")}"
                    })
                .ToListAsync();
            items.AddRange(trat);

            // REPRODUCCIÓN: Registro base
            var repro = await _context.RegistroReproduccions.AsNoTracking()
                .Where(x => x.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.fechaRegistro,
                    Orden = x.Id,
                    Fuente = "Reproducción",
                    Evento = "Registro reproducción",
                    Detalle = $"Registro #{x.Id}"
                })
                .ToListAsync();
            items.AddRange(repro);

            // REPRODUCCIÓN: PARTOS
            var partos = await _context.Partos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { p, rr })
                .Where(x => x.rr.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.p.fechaRegistro,
                    Orden = x.p.Id,
                    Fuente = "Reproducción",
                    Evento = "Parto",
                    Detalle = x.p.tipo
                })
                .ToListAsync();
            items.AddRange(partos);

            // REPRODUCCIÓN: ABORTOS
            var abortos = await _context.Abortos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    a => a.idRegistroReproduccion,
                    rr => rr.Id,
                    (a, rr) => new { a, rr })
                .Where(x => x.rr.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.a.fechaRegistro,
                    Orden = x.a.Id,
                    Fuente = "Reproducción",
                    Evento = "Aborto",
                    Detalle = x.a.idCausaAbortoNavigation.Nombre
                })
                .ToListAsync();
            items.AddRange(abortos);

            // REPRODUCCIÓN: Confirmación preñez
            var conf = await _context.ConfirmacionPrenezs.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    c => c.idRegistroReproduccion,
                    rr => rr.Id,
                    (c, rr) => new { c, rr })
                .Where(x => x.rr.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.c.fechaRegistro,
                    Orden = x.c.Id,
                    Fuente = "Reproducción",
                    Evento = "Confirmación preñez",
                    Detalle = x.c.tipo
                })
                .ToListAsync();
            items.AddRange(conf);

            // === REPRODUCCIÓN: SECA ===
            var secas = await _context.Secas.AsNoTracking()   // o _context.Secas
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    s => s.idRegistroReproduccion,
                    rr => rr.Id,
                    (s, rr) => new { s, rr })
                .Where(x => x.rr.idAnimal == id)
                .Select(x => new HistorialMovimientoVm
                {
                    Fecha = x.s.fechaSeca,
                    Orden = x.s.Id,
                    Fuente = "Reproducción",
                    Evento = "Seca",
                    Detalle = x.s.motivo
                })
                .ToListAsync();

            items.AddRange(secas);


            // REPRODUCCIÓN: PREÑEZ (3 fechas posibles)
            var pre = await _context.Prenezs.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { p, rr })
                .Where(x => x.rr.idAnimal == id)
                .ToListAsync();

            foreach (var x in pre)
            {
                if (x.p.fechaCelo.HasValue)
                    items.Add(new HistorialMovimientoVm
                    {
                        Fecha = x.p.fechaCelo.Value.ToDateTime(TimeOnly.MinValue),
                        Orden = x.p.Id,
                        Fuente = "Reproducción",
                        Evento = "Celo",
                        Detalle = "Registrado"
                    });

                if (x.p.fechaInseminacion.HasValue)
                    items.Add(new HistorialMovimientoVm
                    {
                        Fecha = x.p.fechaInseminacion.Value.ToDateTime(TimeOnly.MinValue),
                        Orden = x.p.Id,
                        Fuente = "Reproducción",
                        Evento = "Inseminación",
                        Detalle = "Registrado"
                    });

                if (x.p.fechaDiagnostico.HasValue)
                    items.Add(new HistorialMovimientoVm
                    {
                        Fecha = x.p.fechaDiagnostico.Value.ToDateTime(TimeOnly.MinValue),
                        Orden = x.p.Id,
                        Fuente = "Reproducción",
                        Evento = "Diagnóstico preñez",
                        Detalle = "Registrado"
                    });
            }

            // Orden final: con fecha primero (desc), luego sin fecha por Id (desc)
            items = items
                .OrderByDescending(i => i.Fecha.HasValue)
                .ThenByDescending(i => i.Fecha)
                .ThenByDescending(i => i.Orden)
                .ToList();

            var vm = new HistorialAnimalViewModel
            {
                AnimalId = animal.Id,
                Nombre = animal.nombre,
                Codigo = animal.codigo,
                Movimientos = items
            };

            return PartialView("_HistorialAnimal", vm);
        }

    }
}
