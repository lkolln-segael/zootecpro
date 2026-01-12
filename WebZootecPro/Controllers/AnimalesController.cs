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
        // Solo cargamos Hatos (ya no existe TipoAnimal)
        // ----------------- HELPERS -----------------
        private async Task CargarCombosAsync(Animal? animal = null, int? excludeAnimalId = null)
        {
            var empresa = await GetEmpresaAsync();
            var establo = await _context.Establos.FirstOrDefaultAsync(e => e.EmpresaId == empresa.Id);
            // HATOS
            ViewBag.IdHato = new SelectList(
                await _context.Hatos.AsNoTracking().Where(h => establo.Id == h.EstabloId).OrderBy(h => h.nombre).ToListAsync(),
                "Id", "nombre", animal?.idHato
            );

            // ESTADO (inventario)
            ViewBag.EstadoId = new SelectList(
                await _context.EstadoAnimals.AsNoTracking().OrderBy(e => e.nombre).ToListAsync(),
                "Id", "nombre", animal?.estadoId
            );

            // PROPÓSITO
            ViewBag.PropositoId = new SelectList(
                await _context.PropositoAnimals.AsNoTracking().OrderBy(p => p.nombre).ToListAsync(),
                "Id", "nombre", animal?.propositoId
            );

            // PROCEDENCIA
            ViewBag.ProcedenciaId = new SelectList(
                await _context.ProcedenciaAnimals.AsNoTracking().OrderBy(p => p.nombre).ToListAsync(),
                "Id", "nombre", animal?.procedenciaId
            );

            // RAZA
            ViewBag.IdRaza = new SelectList(
                await _context.Razas.AsNoTracking().OrderBy(r => r.nombre).ToListAsync(),
                "Id", "nombre", animal?.idRaza
            );

            // ESTADO PRODUCTIVO
            ViewBag.EstadoProductivoId = new SelectList(
                await _context.EstadoProductivos.AsNoTracking().OrderBy(e => e.nombre).ToListAsync(),
                "Id", "nombre", animal?.estadoProductivoId
            );

            ViewBag.IdCategoriaAnimal = new SelectList(
                await _context.CategoriaAnimals.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(),
                "IdCategoriaAnimal", "Nombre", animal?.IdCategoriaAnimal
            );

            // PADRES / MADRES
            var padres = await _context.Animals.AsNoTracking()
                .Where(a => a.idHatoNavigation.EstabloId == establo.Id && a.sexo == "MACHO" && (excludeAnimalId == null || a.Id != excludeAnimalId))
                .OrderBy(a => a.codigo)
                .Select(a => new
                {
                    a.Id,
                    Label = (a.codigo ?? "-") + " - " + (a.nombre ?? "-")
                })
                .ToListAsync();

            var madres = await _context.Animals.AsNoTracking()
                .Where(a => a.sexo == "HEMBRA" && (excludeAnimalId == null || a.Id != excludeAnimalId))
                .OrderBy(a => a.codigo)
                .Select(a => new
                {
                    a.Id,
                    Label = (a.codigo ?? "-") + " - " + (a.nombre ?? "-")
                })
                .ToListAsync();

            ViewBag.IdPadre = new SelectList(padres, "Id", "Label", animal?.idPadre);
            ViewBag.IdMadre = new SelectList(madres, "Id", "Label", animal?.idMadre);
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
                return await _context.Hatos
                    .AnyAsync(h => h.Id == hatoId && h.Establo.Empresa.usuarioID == userId.Value);
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
            // 1) Diccionario: partos por animal (Parto -> RegistroReproduccion -> Animal)
            var usuarioId = GetCurrentUserId();
            var usuario = await GetCurrentUserAsync();
            var empresa = await GetEmpresaAsync();
            var establo = await _context.Establos.FirstOrDefaultAsync(e => e.EmpresaId == empresa.Id);
            var partosPorAnimal = await _context.Partos
                .AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { rr.idAnimal })
                .GroupBy(x => x.idAnimal)
                .Select(g => new { AnimalId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.AnimalId, x => x.Total);

            // 2) Diccionario: hatos
            var hatos = await _context.Hatos
                .AsNoTracking()
                .Where(h => empresa != null && establo.Id == h.Id
                    || usuario.RolId == 1)
                .ToDictionaryAsync(h => h.Id, h => h.nombre);

            // 3) Animales con padres para abuelos
            var animales = await _context.Animals
                .Where(a => hatos.Keys.Contains(a.idHato))
                .Where(a => a.sexo != null && a.sexo.Trim().ToUpper() == "HEMBRA")
                .AsNoTracking()
                .Include(a => a.idPadreNavigation)               // Padre
                .ThenInclude(p => p!.idPadreNavigation)         // Abuelo paterno
                .Include(a => a.idMadreNavigation)              // Madre
                .ThenInclude(m => m!.idPadreNavigation)         // Abuelo materno
                .OrderBy(a => a.codigo)
                .ToListAsync();

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

            // IMPORTANTE: solo devolvemos la lista, no metemos el historial aquí
            return View(model);
        }


        // ----------------- DETAILS -----------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var animal = await _context.Animals
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

            // Navegaciones (por scaffold)
            ModelState.Remove(nameof(Animal.idHatoNavigation));

            // Si por alguna razón no existen los maestros:
            if (animal.estadoId == null)
                ModelState.AddModelError(nameof(Animal.estadoId), "No existe el estado ACTIVO en EstadoAnimal.");
            if (animal.procedenciaId == null)
                ModelState.AddModelError(nameof(Animal.procedenciaId), "No existe la procedencia DESCONOCIDA en ProcedenciaAnimal.");

            animal.nombre = (animal.nombre ?? "").Trim();
            animal.codigo = (animal.codigo ?? "").Trim();
            animal.arete = (animal.arete ?? "").Trim();

            // --- ARETE obligatorio + único ---
            if (string.IsNullOrWhiteSpace(animal.arete))
                ModelState.AddModelError(nameof(animal.arete), "Ingrese arete.");

            if (!string.IsNullOrWhiteSpace(animal.arete))
            {
                var existeArete = await _context.Animals.AnyAsync(a => a.arete == animal.arete);
                if (existeArete)
                    ModelState.AddModelError(nameof(animal.arete), "El arete ya existe, ingrese otro.");
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

            var animal = await _context.Animals.FindAsync(id);
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
            if (id != model.Id)
                return NotFound();

            ModelState.Remove(nameof(Animal.idHatoNavigation));


            model.arete = (model.arete ?? "").Trim();

            // --- ARETE obligatorio ---
            if (string.IsNullOrWhiteSpace(model.arete))
                ModelState.AddModelError(nameof(model.arete), "Ingrese arete.");

            // Si falla la validación, hay que recargar combos (ajusta nombres si tus acciones los usan distinto)
            void CargarCombos()
            {
                ViewData["idHato"] = new SelectList(_context.Hatos, "Id", "nombre", model.idHato);
                ViewData["idPadre"] = new SelectList(_context.Animals, "Id", "nombre", model.idPadre);
                ViewData["idMadre"] = new SelectList(_context.Animals, "Id", "nombre", model.idMadre);
                ViewData["estadoId"] = new SelectList(_context.EstadoAnimals, "Id", "nombre", model.estadoId);
                ViewData["propositoId"] = new SelectList(_context.PropositoAnimals, "Id", "nombre", model.propositoId);
                ViewData["idRaza"] = new SelectList(_context.Razas, "Id", "nombre", model.idRaza);
                ViewData["procedenciaId"] = new SelectList(_context.ProcedenciaAnimals, "Id", "nombre", model.procedenciaId);
                ViewData["estadoProductivoId"] = new SelectList(_context.EstadoProductivos, "Id", "nombre", model.estadoProductivoId);
                ViewData["IdCategoriaAnimal"] = new SelectList(_context.CategoriaAnimals, "IdCategoriaAnimal", "Nombre", model.IdCategoriaAnimal);
            }

            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(model);
            }

            // Traer el registro real de BD (clave para NO sobreescribir con null lo que no venga del form)
            var animalDb = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
            if (animalDb == null)
                return NotFound();

            // Validación de ARETE único (si no está vacío)
            var existeArete = await _context.Animals
    .AnyAsync(a => a.Id != id && a.arete == model.arete);

            if (existeArete)
            {
                ModelState.AddModelError("arete", "El arete ya existe, ingrese otro.");
                CargarCombos();
                return View(model);
            }


            // Actualiza SOLO tus campos
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
            //animalDb.estadoProductivoId = model.estadoProductivoId;
            animalDb.IdCategoriaAnimal = model.IdCategoriaAnimal;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                var existe = await _context.Animals.AnyAsync(e => e.Id == id);
                if (!existe) return NotFound();
                throw;
            }
        }


        // ----------------- DELETE -----------------
        [HttpGet]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == id);

            if (animal == null) return NotFound();

            return View(animal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
            var animal = await _context.Animals
                .AsNoTracking()
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
