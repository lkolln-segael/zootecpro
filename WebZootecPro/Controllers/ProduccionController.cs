using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Produccion;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR,LABORATORIO_EMPRESA,USUARIO_EMPRESA")]
  public class ProduccionController : Controller
  {
    private readonly ZootecContext _context;

    public ProduccionController(ZootecContext context)
    {
      _context = context;
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



    private void CargarComboFuentes(string? selected = null)
    {
      var fuentes = new List<SelectListItem>
              {
                new SelectListItem { Value = "GLORIA", Text = "GLORIA" },
                new SelectListItem { Value = "LAIVE",  Text = "LAIVE"  },
                new SelectListItem { Value = "OTRA",   Text = "OTRA"   },
              };

      ViewBag.Fuentes = new SelectList(fuentes, "Value", "Text", selected);
    }

    private async Task<Empresa?> GetEmpresaAsync()
    {
      var usuarioId = GetCurrentUserId();
      var usuario = await GetCurrentUserAsync();
      return await _context.Empresas.FirstOrDefaultAsync(e => e.usuarioID == usuarioId
          || e.Colaboradors.Select(e => e.idUsuario).Contains(usuarioId.Value));
    }
    // GET: Produccion
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, int? idAnimal)
    {
      var userId = GetCurrentUserId();
      if (userId == null) return Forbid();

      // empresas scope
      List<int> empresaIds;

      if (User.IsInRole("SUPERADMIN"))
      {
        empresaIds = new List<int>(); // sin filtro
      }
      else if (User.IsInRole("ADMIN_EMPRESA"))
      {
        empresaIds = await _context.Empresas.AsNoTracking()
            .Where(e => e.usuarioID == userId.Value)
            .Select(e => e.Id)
            .ToListAsync();
      }
      else
      {
        empresaIds = await _context.Colaboradors.AsNoTracking()
            .Where(c => c.idUsuario == userId.Value)
            .Select(c => c.EmpresaId.Value)
            .Distinct()
            .ToListAsync();
      }

      if (!User.IsInRole("SUPERADMIN") && empresaIds.Count == 0)
        return View(new List<RegistroProduccionLeche>());

      // restricción por establo/hato del usuario (si existe)
      var u = await _context.Usuarios.AsNoTracking()
          .Select(x => new { x.Id, x.idEstablo, x.idHato })
          .FirstOrDefaultAsync(x => x.Id == userId.Value);

      var q = _context.RegistroProduccionLeches
          .AsNoTracking()
          .Include(r => r.idAnimalNavigation)
          .Include(r => r.Calidads)
          .AsQueryable();

      if (!User.IsInRole("SUPERADMIN"))
        q = q.Where(r => empresaIds.Contains(r.idAnimalNavigation.idHatoNavigation.Establo.EmpresaId));

      if (u?.idHato != null)
        q = q.Where(r => r.idAnimalNavigation.idHato == u.idHato.Value);
      else if (u?.idEstablo != null)
        q = q.Where(r => r.idAnimalNavigation.idHatoNavigation.EstabloId == u.idEstablo.Value);

      if (desde.HasValue)
        q = q.Where(r => r.fechaOrdeno >= desde.Value.Date);

      if (hasta.HasValue)
        q = q.Where(r => r.fechaOrdeno <= hasta.Value.Date.AddDays(1).AddTicks(-1));

      if (idAnimal.HasValue)
      {
        // evita filtrar por animal ajeno
        if (!await AnimalVisibleAsync(idAnimal.Value)) return NotFound();
        q = q.Where(r => r.idAnimal == idAnimal.Value);
      }

      var lista = await q
          .OrderByDescending(r => r.fechaOrdeno)
          .ThenBy(r => r.turno)
          .ToListAsync();

      await CargarComboAnimales(idAnimal);
      ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
      ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

      return View(lista);
    }


    // GET: Produccion/Registrar
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR,USUARIO_EMPRESA")]
    public async Task<IActionResult> Registrar(int? idAnimal)
    {
      if (idAnimal.HasValue && !await AnimalVisibleAsync(idAnimal.Value))
        return NotFound();


      var vm = new RegistrarProduccionViewModel
      {
        FechaOrdeno = DateTime.Today,
        Turno = "MAÑANA"
      };

      if (idAnimal.HasValue)
      {
        vm.IdAnimal = idAnimal.Value;
        vm.DiasEnLeche = await CalcularDiasEnLecheAsync(idAnimal.Value, vm.FechaOrdeno);
      }

      await CargarComboAnimales(idAnimal);
      CargarComboFuentes(vm.Fuente);

      return View(vm);
    }

    // Endpoint AJAX para refrescar DEL en pantalla
    [HttpGet]
    public async Task<IActionResult> CalcularDel(int idAnimal, DateTime fechaOrdeno)
    {
      if (!await AnimalVisibleAsync(idAnimal)) return Forbid();

      var del = await CalcularDiasEnLecheAsync(idAnimal, fechaOrdeno);
      return Json(new { diasEnLeche = del });
    }

    // POST: Produccion/Registrar
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR,USUARIO_EMPRESA")]
    public async Task<IActionResult> Registrar(RegistrarProduccionViewModel vm)
    {
      if (!ModelState.IsValid)
      {
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }

      if (!vm.IdAnimal.HasValue || !await AnimalVisibleAsync(vm.IdAnimal.Value))
      {
        return NotFound();
      }

      var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == vm.IdAnimal);
      if (animal == null)
      {
        ModelState.AddModelError(nameof(vm.IdAnimal), "Animal no existe.");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }
      // ✅ Bloquear producción si el animal está en SECA
      var idEstadoSeca = await _context.EstadoProductivos
          .AsNoTracking()
          .Where(e => e.nombre == "SECA")
          .Select(e => (int?)e.Id)
          .FirstOrDefaultAsync();

      if (idEstadoSeca != null && animal.estadoProductivoId == idEstadoSeca.Value)
      {
        ModelState.AddModelError("", "No se puede registrar producción: el animal está en estado SECA (no se ordeña).");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);
        return View(vm);
      }



      // Construye DateTime secuencial (soporta cruce de medianoche en turno NOCHE)
      var fases = BuildPhaseDateTimes(vm.FechaOrdeno,
          vm.HoraPreparacion,
          vm.HoraLimpieza,
          vm.HoraDespunte,
          vm.HoraColocacionPezoneras,
          vm.HoraInicio,
          vm.HoraFin
      );

      var fechaPrep = fases[0];
      var fechaLimp = fases[1];
      var fechaDesp = fases[2];
      var fechaColo = fases[3];
      var fechaInicioOrdeno = fases[4];
      var fechaFinOrdeno = fases[5];

      if (fechaPrep > fechaLimp)
      {
        ModelState.AddModelError("HoraPreparacion", "Fecha de preparacion esta despues de la fecha de limpieza");
        Console.WriteLine("Hora de preparacion mal enviada");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }
      if (fechaLimp > fechaDesp)
      {
        ModelState.AddModelError("HoraLimpieza", "Fecha de despojo esta despues de la fecha de limpieza");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }
      if (fechaDesp > fechaColo)
      {
        ModelState.AddModelError("HoraDespunte", "Fecha de despunte esta despues de la fecha de colocacion");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }
      if (fechaColo > fechaInicioOrdeno)
      {
        ModelState.AddModelError("HoraInicio", "Fecha de colocacion esta despues de la fecha de inicio de ordeno");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }
      if (fechaInicioOrdeno > fechaFinOrdeno)
      {
        ModelState.AddModelError("HoraColocacionPezoneras", "Fecha de inicio de ordeno esta despues de la fecha de fin de ordeno");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }

      if (fechaPrep == null || fechaLimp == null || fechaDesp == null || fechaColo == null || fechaInicioOrdeno == null || fechaFinOrdeno == null)
      {
        ModelState.AddModelError("HoraInicio", "Debes completar todas las horas de fases e inicio/fin.");
        await CargarComboAnimales(vm.IdAnimal);
        CargarComboFuentes(vm.Fuente);

        return View(vm);
      }

      // DEL (Días en leche) = días desde el último PARTO registrado
      var diasEnLeche = await CalcularDiasEnLecheAsync(animal.Id, vm.FechaOrdeno);
      vm.DiasEnLeche = diasEnLeche;

      // ✅ SANIDAD -> PRODUCCIÓN: Retiro de leche
      var retiroHasta = await GetRetiroLecheHastaAsync(animal.Id, fechaInicioOrdeno.Value);

      if (retiroHasta != null)
      {
        // Auto-aplica descarte y marca antibiótico
        AplicarRetiroLecheEnProduccion(vm, retiroHasta.Value);

        // Alerta
        TempData["ProduccionMessage"] =
            $"⚠️ Animal en RETIRO DE LECHE hasta {retiroHasta.Value:dd/MM/yyyy}. " +
            $"Se movió INDUSTRIA/VENTA a DESCARTE y se marcó antibiótico.";
      }

      var entidad = new RegistroProduccionLeche
      {
        idAnimal = animal.Id,

        fechaPreparacion = fechaPrep,
        fechaLimpieza = fechaLimp,
        fechaDespunte = fechaDesp,
        fechaColocacionPezoneras = fechaColo,

        fechaOrdeno = fechaInicioOrdeno.Value,
        fechaRetirada = fechaFinOrdeno.Value,

        pesoOrdeno = vm.CantidadTotal,
        turno = vm.Turno,

        cantidadIndustria = vm.CantidadIndustria,
        cantidadTerneros = vm.CantidadTerneros,
        cantidadDescartada = vm.CantidadDescartada,
        cantidadVentaDirecta = vm.CantidadVentaDirecta,

        tieneAntibiotico = vm.TieneAntibiotico,
        motivoDescarte = vm.MotivoDescarte,
        diasEnLeche = diasEnLeche,
        fechaRegistro = DateTime.Now,
        fuente = vm.Fuente,

      };

      _context.Add(entidad);
      // Guardar calidad si mandaron algún dato
      bool puedeCargarCalidad =
       User.IsInRole("SUPERADMIN") ||
       User.IsInRole("ADMIN_EMPRESA") ||
       User.IsInRole("LABORATORIO_EMPRESA");

      if (puedeCargarCalidad &&
          (vm.Grasa.HasValue || vm.Proteina.HasValue || vm.SolidosTotales.HasValue || vm.Urea.HasValue || vm.Rcs.HasValue))
      {

        if (vm.Grasa < 0 && vm.Grasa > 100)
        {
          ModelState.AddModelError("Grasa", "La grasa tiene que ser un porcentaje");
          await CargarComboAnimales(vm.IdAnimal);
          CargarComboFuentes(vm.Fuente);

          return View(vm);
        }
        if (vm.Proteina < 0 && vm.Proteina > 100)
        {
          ModelState.AddModelError("Proteina", "La proteina tiene que ser un porcentaje");
          await CargarComboAnimales(vm.IdAnimal);
          CargarComboFuentes(vm.Fuente);

          return View(vm);
        }
        if (vm.SolidosTotales < 0 && vm.SolidosTotales > 100)
        {
          ModelState.AddModelError("Grasa", "Los solidos totales tienen que ser un porcentaje");
          await CargarComboAnimales(vm.IdAnimal);
          CargarComboFuentes(vm.Fuente);

          return View(vm);
        }

        var calidad = new Calidad
        {
          idRegistroProduccionLeche = entidad.Id,
          grasa = vm.Grasa,
          proteina = vm.Proteina,
          solidosTotales = vm.SolidosTotales,
          urea = vm.Urea,
          rcs = vm.Rcs,
          fechaRegistro = DateTime.Now
        };

        _context.Calidads.Add(calidad);
      }
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }


    private async Task CargarComboAnimales(int? idSeleccionado = null)
    {
      var limite = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
      var empresa = await GetEmpresaAsync();
      var animales = await _context.Animals
          .OrderBy(a => a.codigo)
          .Where(a => a.fechaNacimiento.HasValue
              && a.fechaNacimiento.Value < limite
              && a.idHatoNavigation.Establo.EmpresaId == empresa.Id)
          .Select(a => new
          {
            a.Id,
            Descripcion = (a.codigo ?? "-") + " - " + (a.nombre ?? "-")
          })
          .ToListAsync();

      ViewBag.IdAnimal = new SelectList(animales, "Id", "Descripcion", idSeleccionado);
    }

    // Construye DateTime por fase en orden (si la hora "baja", suma +1 día)
    private static DateTime?[] BuildPhaseDateTimes(DateTime baseDate, params TimeSpan?[] horas)
    {
      var result = new DateTime?[horas.Length];
      DateTime? last = null;

      for (int i = 0; i < horas.Length; i++)
      {
        if (!horas[i].HasValue)
        {
          result[i] = null;
          continue;
        }

        var dt = baseDate.Date + horas[i]!.Value;

        // si cruza medianoche respecto al anterior, empuja al día siguiente
        var esMediaNoche = last.HasValue && dt.Hour == 0 && last.Value.Hour == 23;
        if (esMediaNoche)
          dt = dt.AddDays(1);

        result[i] = dt;
        last = dt;
      }

      return result;
    }

    // DEL: desde último parto (Parto.fechaRegistro) del animal
    private async Task<int?> CalcularDiasEnLecheAsync(int idAnimal, DateTime fechaOrdeno)
    {
      // Último parto del animal: Parto -> RegistroReproduccion (que tiene idAnimal)
      var ultimoParto = await _context.Partos.AsNoTracking()
          .Join(_context.RegistroReproduccions.AsNoTracking(),
                p => p.idRegistroReproduccion,
                rr => rr.Id,
                (p, rr) => new { p.fechaRegistro, rr.idAnimal })
          .Where(x => x.idAnimal == idAnimal)
          .OrderByDescending(x => x.fechaRegistro)
          .Select(x => (DateTime?)x.fechaRegistro)
          .FirstOrDefaultAsync();

      if (ultimoParto == null)
        return null;

      var del = (fechaOrdeno.Date - ultimoParto.Value.Date).Days;
      if (del < 0) del = 0;
      return del;
    }

    [HttpGet]
    public async Task<IActionResult> GetRetiroLeche(int idAnimal, DateTime fechaOrdeno)
    {
      if (!await AnimalVisibleAsync(idAnimal)) return Forbid();
      // usamos mediodía para evitar temas de hora 00:00
      var dt = fechaOrdeno.Date.AddHours(12);
      var hasta = await GetRetiroLecheHastaAsync(idAnimal, dt);

      return Json(new
      {
        enRetiro = hasta != null,
        retiroHasta = hasta?.ToString("dd/MM/yyyy")
      });
    }

    private async Task<DateTime?> GetRetiroLecheHastaAsync(int idAnimal, DateTime fechaOrdeno)
    {
      var data = await _context.Tratamientos
          .AsNoTracking()
          .Include(t => t.idTipoTratamientoNavigation)
          .Include(t => t.idEnfermedadNavigation)
          .Where(t =>
              t.idEnfermedadNavigation.idAnimal == idAnimal &&
              t.idTipoTratamientoNavigation.retiroLecheDias != null &&
              t.idTipoTratamientoNavigation.retiroLecheDias.Value > 0)
          .Select(t => new
          {
            Inicio = t.fechaInicio,
            UltimaDosis = (t.fechaFinalEstimada ?? t.fechaInicio),
            RetiroDias = t.idTipoTratamientoNavigation.retiroLecheDias!.Value
          })
          .ToListAsync();

      DateTime? maxHasta = null;

      foreach (var x in data)
      {
        // Si el ordeño es antes del tratamiento, no aplica.
        if (fechaOrdeno.Date < x.Inicio.Date) continue;

        // Fin de retiro (inclusive) al final del día
        var hasta = x.UltimaDosis.Date
            .AddDays(x.RetiroDias)
            .AddDays(1)
            .AddTicks(-1);

        if (maxHasta == null || hasta > maxHasta.Value)
          maxHasta = hasta;
      }

      // Solo devuelve si realmente está en retiro en esa fecha
      if (maxHasta != null && fechaOrdeno <= maxHasta.Value)
        return maxHasta;

      return null;
    }

    private static void AplicarRetiroLecheEnProduccion(RegistrarProduccionViewModel vm, DateTime retiroHasta)
    {
      // Marca antibiótico sí o sí
      vm.TieneAntibiotico = true;

      // Mover INDUSTRIA + VENTA DIRECTA -> DESCARTE
      var movido = 0m;

      if (vm.CantidadIndustria > 0)
      {
        movido += vm.CantidadIndustria;
        vm.CantidadIndustria = 0;
      }

      if (vm.CantidadVentaDirecta > 0)
      {
        movido += vm.CantidadVentaDirecta;
        vm.CantidadVentaDirecta = 0;
      }

      if (movido > 0)
        vm.CantidadDescartada += movido;

      // Motivo
      if (string.IsNullOrWhiteSpace(vm.MotivoDescarte))
        vm.MotivoDescarte = $"RETIRO DE LECHE por tratamiento (hasta {retiroHasta:dd/MM/yyyy})";
    }

    private async Task CargarComboHatos(int? idSeleccionado = null)
    {
      var empresa = await GetEmpresaAsync();
      var hatos = await _context.Hatos
          .OrderBy(h => h.nombre)
          .Where(h => h.Establo.EmpresaId == empresa.Id)
          .Select(h => new { h.Id, h.nombre })
          .ToListAsync();

      ViewBag.IdHato = new SelectList(hatos, "Id", "nombre", idSeleccionado);
    }

    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR,USUARIO_EMPRESA")]
    public async Task<IActionResult> ReporteIndustria(DateTime? desde, DateTime? hasta, int? idHato)
    {
      // Convertimos DateTime? -> DateOnly? para filtrar en SQL sin ToDateTime()
      DateOnly? dDesde = desde.HasValue ? DateOnly.FromDateTime(desde.Value) : null;
      DateOnly? dHasta = hasta.HasValue ? DateOnly.FromDateTime(hasta.Value) : null;

      var empresa = await GetEmpresaAsync();

      // Query base (todo en SQL)
      var q = _context.ReporteIndustriaLeches
          .AsNoTracking()
          .Where(r => r.idHatoNavigation.Establo.EmpresaId == empresa.Id)
          .Join(_context.Hatos.AsNoTracking(),
              r => r.idHato,
              h => h.Id,
              (r, h) => new
              {
                r.Id,
                r.fecha,        // DateOnly
                r.turno,
                r.idHato,
                HatoNombre = h.nombre,
                r.pesoReportado,
                r.observacion
              });

      if (dDesde.HasValue) q = q.Where(x => x.fecha >= dDesde.Value);
      if (dHasta.HasValue) q = q.Where(x => x.fecha <= dHasta.Value);
      if (idHato.HasValue) q = q.Where(x => x.idHato == idHato.Value);

      // Traemos lista en memoria y armamos el ViewModel (aquí sí ToDateTime)
      var lista = await q
          .OrderByDescending(x => x.fecha)
          .ThenBy(x => x.turno)
          .ThenBy(x => x.HatoNombre)
          .Select(x => new ReporteIndustriaRowViewModel
          {
            Id = x.Id,
            Fecha = x.fecha.ToDateTime(TimeOnly.MinValue), // ya fuera de EF
            Turno = x.turno,
            IdHato = x.idHato,
            HatoNombre = x.HatoNombre,
            PesoReportado = x.pesoReportado,
            Observacion = x.observacion
          })
          .ToListAsync();

      await CargarComboHatos(idHato);

      ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
      ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

      return View(lista);
    }


    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR")]
    public async Task<IActionResult> RegistrarReporteIndustria(int? id)
    {
      await CargarComboHatos();

      if (id == null)
        return View(new ReporteIndustriaFormViewModel());

      var ent = await _context.ReporteIndustriaLeches.FindAsync(id.Value);
      if (ent == null) return NotFound();

      return View(new ReporteIndustriaFormViewModel
      {
        Id = ent.Id,
        Fecha = ent.fecha.ToDateTime(TimeOnly.MinValue),
        Turno = ent.turno,
        IdHato = ent.idHato,
        PesoReportado = ent.pesoReportado,
        Observacion = ent.observacion
      });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR")]
    public async Task<IActionResult> RegistrarReporteIndustria(ReporteIndustriaFormViewModel vm)
    {
      if (!ModelState.IsValid)
      {
        await CargarComboHatos(vm.IdHato);
        return View(vm);
      }

      // Evita duplicado por UNIQUE(fecha,turno,idHato)
      var fechaDo = DateOnly.FromDateTime(vm.Fecha.Date);

      if (vm.PesoReportado <= 0)
      {
        ModelState.AddModelError("PesoReportado", "El peso reportado tiene que ser positivo");
        await CargarComboHatos(vm.IdHato);
        return View(vm);
      }

      if (vm.Id == null)
      {
        var ent = new ReporteIndustriaLeche
        {
          fecha = fechaDo,
          turno = vm.Turno,
          idHato = vm.IdHato,
          pesoReportado = vm.PesoReportado,
          observacion = vm.Observacion,
          fechaRegistro = DateTime.Now
        };

        _context.ReporteIndustriaLeches.Add(ent);
      }
      else
      {
        var ent = await _context.ReporteIndustriaLeches.FindAsync(vm.Id.Value);
        if (ent == null) return NotFound();

        ent.fecha = fechaDo;
        ent.turno = vm.Turno;
        ent.idHato = vm.IdHato;
        ent.pesoReportado = vm.PesoReportado;
        ent.observacion = vm.Observacion;
      }

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateException)
      {
        ModelState.AddModelError("", "Ya existe un reporte para esa FECHA + TURNO + HATO.");
        await CargarComboHatos(vm.IdHato);
        return View(vm);
      }

      return RedirectToAction(nameof(ReporteIndustria));
    }

    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,INSPECTOR,USUARIO_EMPRESA")]
    public async Task<IActionResult> ComparativoIndustria(DateTime? desde, DateTime? hasta, int? idHato)
    {
      var empresa = await GetEmpresaAsync();
      var q = _context.RegistroProduccionLeches
          .AsNoTracking()
          .Include(r => r.idAnimalNavigation)
          .Where(r => r.idAnimalNavigation.idHatoNavigation.Establo.EmpresaId == empresa.Id &&
              r.fechaOrdeno != null);

      if (desde.HasValue)
        q = q.Where(r => r.fechaOrdeno >= desde.Value.Date);

      if (hasta.HasValue)
        q = q.Where(r => r.fechaOrdeno <= hasta.Value.Date.AddDays(1).AddTicks(-1));

      if (idHato.HasValue)
        q = q.Where(r => r.idAnimalNavigation.idHato == idHato.Value);

      var prod = await q
          .GroupBy(r => new
          {
            Fecha = r.fechaOrdeno!.Value.Date,
            r.turno,
            IdHato = r.idAnimalNavigation.idHato
          })
          .Select(g => new
          {
            g.Key.Fecha,
            g.Key.turno,
            g.Key.IdHato,
            ProducidoTotal = g.Sum(x => x.pesoOrdeno ?? 0m),
            EntregadoIndustria = g.Sum(x => x.cantidadIndustria ?? 0m)
          })
          .ToListAsync();

      // reportes industria
      var rep = await _context.ReporteIndustriaLeches
          .AsNoTracking()
          .ToListAsync();

      var hatos = await _context.Hatos.AsNoTracking().ToDictionaryAsync(h => h.Id, h => h.nombre);

      var repDict = rep.ToDictionary(
          x => (Fecha: x.fecha.ToDateTime(TimeOnly.MinValue).Date, Turno: x.turno, IdHato: x.idHato),
          x => x.pesoReportado
      );

      var rows = prod
          .Select(p =>
          {
            repDict.TryGetValue((p.Fecha, p.turno, p.IdHato), out var pesoRep);

            return new ComparativoIndustriaRowViewModel
            {
              Fecha = p.Fecha,
              Turno = p.turno,
              Hato = hatos.TryGetValue(p.IdHato, out var nom) ? nom : $"Hato {p.IdHato}",
              ProducidoTotal = p.ProducidoTotal,
              EntregadoIndustria = p.EntregadoIndustria,
              ReportadoIndustria = repDict.ContainsKey((p.Fecha, p.turno, p.IdHato)) ? pesoRep : (decimal?)null
            };
          })
          .OrderByDescending(x => x.Fecha)
          .ThenBy(x => x.Turno)
          .ThenBy(x => x.Hato)
          .ToList();

      await CargarComboHatos(idHato);
      ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
      ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

      return View(rows);
    }

    private async Task<bool> AnimalVisibleAsync(int idAnimal)
    {
      if (User.IsInRole("SUPERADMIN")) return true;

      var userId = GetCurrentUserId();
      if (userId == null) return false;

      // empresa(s) a la que pertenece el usuario:
      // - ADMIN_EMPRESA: dueño (Empresas.usuarioID)
      // - otros roles: colaborador (Colaborador.EmpresaId)
      List<int> empresaIds;

      if (User.IsInRole("ADMIN_EMPRESA"))
      {
        empresaIds = await _context.Empresas.AsNoTracking()
            .Where(e => e.usuarioID == userId.Value)
            .Select(e => e.Id)
            .ToListAsync();
      }
      else
      {
        empresaIds = await _context.Colaboradors.AsNoTracking()
        .Where(c => c.idUsuario == userId.Value && c.EmpresaId.HasValue)
        .Select(c => c.EmpresaId.Value) // Ahora es int en lugar de int?
        .Distinct()
        .ToListAsync();

      }

      if (empresaIds.Count == 0) return false;

      // si el usuario tiene hato/establo asignado, aplica restricción extra
      var u = await _context.Usuarios.AsNoTracking()
          .Select(x => new { x.Id, x.idEstablo, x.idHato })
          .FirstOrDefaultAsync(x => x.Id == userId.Value);

      var q = _context.Animals.AsNoTracking()
          .Where(a => a.Id == idAnimal)
          .Where(a => empresaIds.Contains(a.idHatoNavigation.Establo.EmpresaId));

      if (u?.idHato != null)
        q = q.Where(a => a.idHato == u.idHato.Value);
      else if (u?.idEstablo != null)
        q = q.Where(a => a.idHatoNavigation.EstabloId == u.idEstablo.Value);

      return await q.AnyAsync();
    }
  }
}
