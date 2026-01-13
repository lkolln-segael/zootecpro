using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Eventos;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,VETERINARIO,USUARIO_EMPRESA")]
  public class EventosController : Controller
  {
    private readonly ZootecContext _context;
    private readonly int MINIMA_EDAD_INSEMINACION = 5;
    private bool IsSuperAdmin => User.IsInRole("SUPERADMIN");
    private bool IsAdminEmpresa => User.IsInRole("ADMIN_EMPRESA");

    public EventosController(ZootecContext context)
    {
      _context = context;
    }

    private async Task<IQueryable<Animal>> ScopeAnimalesAsync(IQueryable<Animal> q)
    {
      if (IsSuperAdmin) return q;

      var userId = GetCurrentUserId();
      if (userId == null) return q.Where(_ => false);

      if (IsAdminEmpresa)
      {
        // Dueño o colaborador de la empresa del establo
        return q.Where(a =>
            a.idHatoNavigation.Establo.Empresa.usuarioID == userId.Value
            || a.idHatoNavigation.Establo.Empresa.Colaboradors.Any(c => c.idUsuario == userId.Value)
        );
      }

      // Otros roles: amarrados a Hato o Establo
      var u = await GetCurrentUserAsync();
      if (u != null && u?.idHato != null) return q.Where(a => a.idHato == u.idHato.Value);
      if (u != null && u?.idEstablo != null) return q.Where(a => a.idHatoNavigation.EstabloId == u.idEstablo.Value);

      return q.Where(_ => false);
    }

    private async Task<bool> AnimalEsVisibleAsync(int animalId)
    {
      var q = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
      return await q.AnyAsync(a => a.Id == animalId);
    }
    private async Task<Usuario?> GetUsuarioActualAsync()
    {
      var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (!int.TryParse(userIdStr, out var userId)) return null;

      return await _context.Usuarios
          .Include(u => u.Rol)
          .FirstOrDefaultAsync(u => u.Id == userId);
    }

    private int? GetUsuarioId()
    {
      var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.TryParse(s, out var id) ? id : (int?)null;
    }

    private async Task CargarVeterinariosAsync(RegistrarEventoViewModel vm)
    {
      // 1) ID de Especialidad = VETERINARIO
      var idEspVet = await _context.Especialidads
          .AsNoTracking()
          .Where(e => e.Nombre == "VETERINARIO")
          .Select(e => (int?)e.Id)
          .FirstOrDefaultAsync();

      // Si no existe esa especialidad aún
      if (idEspVet == null)
      {
        ViewBag.IdVeterinario = new List<SelectListItem>();
        return;
      }

      // 2) Usuario actual (para filtrar por su establo/hato si aplica)
      var usuarioId = GetUsuarioId();
      var u = usuarioId == null
          ? null
          : await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == usuarioId.Value);

      // 3) Si el logueado ES veterinario, preseleccionarlo
      if (vm.IdVeterinario == null && usuarioId != null)
      {
        var soyVet = await _context.Colaboradors.AsNoTracking()
            .AnyAsync(c => c.idUsuario == usuarioId.Value && c.EspecialidadId == idEspVet.Value);

        if (soyVet)
          vm.IdVeterinario = usuarioId.Value; // queda seleccionado en el combo
      }

      // 4) Lista de veterinarios (Colaborador) -> Value = UsuarioId, Text = Colaborador.nombre
      var q = _context.Colaboradors
          .AsNoTracking()
          .Include(c => c.idUsuarioNavigation)
          .Where(c => c.EspecialidadId == idEspVet.Value);

      // (Opcional pero recomendado) filtrar por el mismo establo/hato del usuario logueado
      if (u?.idEstablo != null)
        q = q.Where(c => c.idUsuarioNavigation.idEstablo == u.idEstablo);

      if (u?.idHato != null)
        q = q.Where(c => c.idUsuarioNavigation.idHato == u.idHato);

      var veterinarios = await q
          .OrderBy(c => c.nombre)
          .Select(c => new SelectListItem
          {
            Value = c.idUsuario.ToString(),
            Text = c.nombre
          })
          .ToListAsync();

      ViewBag.IdVeterinario = veterinarios;
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
    private async Task<Empresa?> GetEmpresaAsync()
    {
      var usuarioId = GetCurrentUserId();
      var usuario = await GetCurrentUserAsync();
      return await _context.Empresas.FirstOrDefaultAsync(e => e.usuarioID == usuarioId
          || e.Colaboradors.Select(e => e.idUsuario).Contains(usuarioId.Value));
    }

    private async Task CargarCombosAsync(RegistrarEventoViewModel? model = null)
    {
      var animalesQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

      // ✅ Solo ACTIVO para registrar eventos
      var idActivo = await _context.EstadoAnimals.AsNoTracking()
          .Where(x => x.nombre == "ACTIVO")
          .Select(x => (int?)x.Id)
          .FirstOrDefaultAsync();

      if (idActivo != null)
        animalesQ = animalesQ.Where(a => a.estadoId == idActivo.Value);

      var tipo = (model?.TipoEvento ?? "").Trim().ToUpperInvariant();

      // ✅ Solo restringir a HEMBRAS en eventos reproductivos
      var tiposRepro = new HashSet<string>
            {
                "PARTO", "SERVICIO", "CONFIRMACION_PREÑEZ", "SECA", "ABORTO", "CELO"
            };

      if (string.IsNullOrEmpty(tipo) || tiposRepro.Contains(tipo))
      {
        animalesQ = animalesQ.Where(a => a.sexo != null && a.sexo.Trim().ToUpper() == "HEMBRA");
      }

      if (tipo == "CONFIRMACION_PREÑEZ")
      {
        var madresPendientesQ =
            (from p in _context.Prenezs.AsNoTracking()
             where p.fechaInseminacion != null
             join c in _context.ConfirmacionPrenezs.AsNoTracking()
                  on p.idRegistroReproduccion equals c.idRegistroReproduccion into gj
             from c in gj.DefaultIfEmpty()
             where c == null
             select p.idMadreAnimal)
            .Distinct();

        animalesQ = animalesQ.Where(a => madresPendientesQ.Contains(a.Id));
      }

      ViewBag.IdAnimal = await animalesQ
          .OrderBy(a => a.arete)
          .Select(a => new SelectListItem
          {
            Value = a.Id.ToString(),
            Text = (a.arete ?? "-") + " - " + a.nombre
          }).ToListAsync();

      ViewBag.TiposEvento = new List<SelectListItem>
    {
        new("Parto", "PARTO"),
        new("Servicio", "SERVICIO"),
        new("Confirmación preñez", "CONFIRMACION_PREÑEZ"),
        new("Seca", "SECA"),
        new("Aborto", "ABORTO"),

        // ✅ NUEVOS
        new("Venta", "VENTA"),
        new("Muerte", "MUERTE"),
    };

      ViewBag.IdTipoEnfermedad = new SelectList(
          await _context.TipoEnfermedades.OrderBy(t => t.nombre).ToListAsync(),
          "Id", "nombre", model?.IdTipoEnfermedad
      );

      ViewBag.IdCausaAborto = new SelectList(
          await _context.CausaAbortos.Where(x => !x.Oculto).OrderBy(x => x.Nombre).ToListAsync(),
          "Id", "Nombre", model?.IdCausaAborto
      );

      ViewBag.IdEnfermedad = new SelectList(Enumerable.Empty<SelectListItem>());
      ViewBag.IdTipoTratamiento = new SelectList(Enumerable.Empty<SelectListItem>());

      await CargarVeterinariosAsync(model ?? new RegistrarEventoViewModel());
    }



    private async Task CargarCombosServicioAsync(RegistrarEventoViewModel vm)
    {
      ViewBag.Protocolos = new SelectList(new[]
      {
        "Ovsynch", "Presynch-Ovsynch", "CIDR", "IATF"
    }, vm.Protocolo);

      var torosQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
      torosQ = torosQ.Where(a => a.sexo != null && a.sexo.Trim().ToUpper() == "MACHO");
      ViewBag.Toros = await torosQ
          .OrderBy(a => a.nombre)
          .Select(a => new { a.Id, a.nombre, a.arete, a.naab })
          .ToListAsync();
      ViewBag.IdAnimal = await torosQ
          .OrderBy(a => a.nombre)
          .Select(a => new { a.Id, a.nombre, a.arete, a.naab })
          .ToListAsync();
      var empresa = await GetEmpresaAsync();
      var inseminadores = await _context.Inseminadors.Where(i => empresa != null && i.EmpresaId == empresa.Id).ToListAsync();
      ViewBag.Inseminadores = new SelectList(
          inseminadores.Select(i => new Dictionary<string, string> { { "Text", i.nombre + " " + i.apellido }, { "Value", i.Id.ToString() } })
            , "Value", "Text"
          );
    }



    [HttpGet]
    public async Task<IActionResult> Registrar()
    {
      var vm = new RegistrarEventoViewModel
      {
        FechaEvento = DateOnly.FromDateTime(DateTime.Today)
      };

      await CargarCombosAsync(vm);
      return View(vm);
    }

    // ====== ENDPOINT para cargar partial según TipoEvento ======
    // GET /Eventos/CamposEvento
    [HttpGet]
    public async Task<IActionResult> CamposEvento(string tipoEvento, int? idAnimal)
    {
      var vm = new RegistrarEventoViewModel { TipoEvento = tipoEvento, IdAnimal = idAnimal };
      // ✅ seguridad: si viene idAnimal, validar acceso
      if (idAnimal.HasValue && idAnimal.Value > 0)
      {
        if (!await AnimalEsVisibleAsync(idAnimal.Value))
          return Forbid();
      }

      await CargarCombosAsync(vm);

      if (tipoEvento == "PARTO")
      {
        await CargarCombosPartoAsync(vm);
      }



      if (tipoEvento == "SERVICIO")
      {
        await CargarCombosServicioAsync(vm);

      }

      if (tipoEvento == "CONFIRMACION_PREÑEZ" && idAnimal.HasValue && idAnimal.Value > 0)
      {
        var last = await _context.Prenezs
        .AsNoTracking()
        .Where(p => p.idMadreAnimal == idAnimal.Value && p.fechaInseminacion != null)
        .Where(p => !_context.ConfirmacionPrenezs.Any(c => c.idRegistroReproduccion == p.idRegistroReproduccion))
        .OrderByDescending(p => p.fechaInseminacion)
        .Select(p => new { p.fechaInseminacion, p.fechaProbableParto })
        .FirstOrDefaultAsync();


        if (last != null)
        {
          var fInsem = last.fechaInseminacion!.Value;
          var fProb = last.fechaProbableParto ?? fInsem.AddDays(279);
          var fSeca = fProb.AddDays(-60);

          ViewBag.FechaInseminacion = fInsem.ToString("dd/MM/yyyy");
          ViewBag.FechaProbParto = fProb.ToString("dd/MM/yyyy");
          ViewBag.FechaSecaCalc = fSeca.ToString("dd/MM/yyyy");
        }
      }

      if (tipoEvento == "SECA" && idAnimal.HasValue && idAnimal.Value > 0)
      {
        var ciclo = await (
            from p in _context.Prenezs
            join c in _context.ConfirmacionPrenezs on p.idRegistroReproduccion equals c.idRegistroReproduccion
            where p.idMadreAnimal == idAnimal.Value
                  && c.tipo == "POSITIVA"
                  && !_context.Partos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                  && !_context.Abortos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
            orderby c.fechaRegistro descending
            select new { p.fechaInseminacion, p.fechaProbableParto }
        ).FirstOrDefaultAsync();

        if (ciclo != null)
        {
          var fInsem = ciclo.fechaInseminacion!.Value;
          var fProb = ciclo.fechaProbableParto ?? fInsem.AddDays(279);
          var fSeca = fProb.AddDays(-60);

          ViewBag.FechaProbParto = fProb.ToString("dd/MM/yyyy");
          ViewBag.FechaSecaCalc = fSeca.ToString("dd/MM/yyyy");
        }
      }

      if (tipoEvento == "ABORTO" && idAnimal.HasValue && idAnimal.Value > 0)
      {
        var ciclo = await (
            from p in _context.Prenezs
            join c in _context.ConfirmacionPrenezs on p.idRegistroReproduccion equals c.idRegistroReproduccion
            where p.idMadreAnimal == idAnimal.Value
                  && c.tipo == "POSITIVA"
                  && !_context.Partos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                  && !_context.Abortos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
            orderby c.fechaRegistro descending
            select new { p.fechaInseminacion }
        ).FirstOrDefaultAsync();

        if (ciclo?.fechaInseminacion != null)
          ViewBag.FechaInseminacionISO = ciclo.fechaInseminacion.Value.ToString("yyyy-MM-dd");
      }


      // ✅ usar el helper completo que ya tienes
      if (tipoEvento == "MEDICACION")
        await PrepararMedicacionAsync(vm);

      return tipoEvento switch
      {
        "PARTO" => PartialView("_CamposParto", vm),
        "SERVICIO" => PartialView("_CamposServicio", vm),
        "CONFIRMACION_PREÑEZ" => PartialView("_CamposConfirmacionPrenez", vm),
        "SECA" => PartialView("_CamposSeca", vm),
        "ABORTO" => PartialView("_CamposAborto", vm),
        "VENTA" => PartialView("_CamposSalida", vm),
        "MUERTE" => PartialView("_CamposSalida", vm),
        /*"ENFERMEDAD" => PartialView("_CamposEnfermedad", vm),
        "MEDICACION" => PartialView("_CamposMedicacion", vm),*/
        // "PRODUCCION_LECHE" => PartialView("_CamposProduccionLeche", vm),
        _ => PartialView("_CamposVacios", vm)
      };
    }
    private async Task PrepararMedicacionAsync(RegistrarEventoViewModel vm)
    {
      if (vm.IdAnimal == null)
      {
        ViewBag.IdEnfermedad = new SelectList(Enumerable.Empty<SelectListItem>());
        ViewBag.IdTipoTratamiento = new SelectList(Enumerable.Empty<SelectListItem>());
        return;
      }

      if (!await AnimalEsVisibleAsync(vm.IdAnimal.Value))
      {
        ViewBag.IdEnfermedad = new SelectList(Enumerable.Empty<SelectListItem>());
        ViewBag.IdTipoTratamiento = new SelectList(Enumerable.Empty<SelectListItem>());
        return;
      }

      var casos = await _context.Enfermedads
          .AsNoTracking()
          .Include(e => e.idTipoEnfermedadNavigation)
          .Where(e => e.idAnimal == vm.IdAnimal.Value && e.fechaRecuperacion == null)
          .OrderByDescending(e => e.fechaDiagnostico)
          .Select(e => new
          {
            e.Id,
            e.idTipoEnfermedad,
            Texto = $"{e.idTipoEnfermedadNavigation.nombre} ({e.fechaDiagnostico:dd/MM/yyyy})"
          })
          .ToListAsync();

      // si te mandan un IdEnfermedad que no pertenece al animal, lo ignoras
      var casoSel = (vm.IdEnfermedad != null && casos.Any(x => x.Id == vm.IdEnfermedad.Value))
          ? vm.IdEnfermedad
          : casos.FirstOrDefault()?.Id;

      vm.IdEnfermedad = casoSel;

      ViewBag.IdEnfermedad = new SelectList(casos, "Id", "Texto", casoSel);

      if (casoSel == null)
      {
        ViewBag.IdTipoTratamiento = new SelectList(Enumerable.Empty<SelectListItem>());
        return;
      }

      var idTipoEnf = casos.First(x => x.Id == casoSel.Value).idTipoEnfermedad;

      var tratamientos = await _context.TipoTratamientos
          .AsNoTracking()
          .Where(t => t.idTipoEnfermedad == idTipoEnf)
          .OrderBy(t => t.nombre)
          .ToListAsync();

      var tratSel = vm.IdTipoTratamiento ?? tratamientos.FirstOrDefault()?.Id;
      vm.IdTipoTratamiento = tratSel;

      ViewBag.IdTipoTratamiento = new SelectList(tratamientos, "Id", "nombre", tratSel);
    }



    // Para medicación: cargar tratamientos según tipo enfermedad
    [HttpGet]
    public async Task<IActionResult> TiposTratamientoPorEnfermedad(int idTipoEnfermedad)
    {
      var data = await _context.TipoTratamientos
          .Where(t => t.idTipoEnfermedad == idTipoEnfermedad)
          .OrderBy(t => t.nombre)
          .Select(t => new { value = t.Id, text = t.nombre })
          .ToListAsync();

      return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> TiposTratamientoPorCaso(int idEnfermedad)
    {
      var animalesScope = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

      var idTipoEnfermedad = await (
          from e in _context.Enfermedads.AsNoTracking()
          join a in animalesScope on e.idAnimal equals a.Id
          where e.Id == idEnfermedad
          select (int?)e.idTipoEnfermedad
      ).FirstOrDefaultAsync();

      if (idTipoEnfermedad == null)
        return Json(Array.Empty<object>());

      var data = await _context.TipoTratamientos
          .AsNoTracking()
          .Where(t => t.idTipoEnfermedad == idTipoEnfermedad.Value)
          .OrderBy(t => t.nombre)
          .Select(t => new { value = t.Id, text = t.nombre })
          .ToListAsync();

      return Json(data);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarEventoViewModel model)
    {
      if (!ModelState.IsValid)
      {
        await CargarCombosAsync(model);
        if (model.TipoEvento == "SERVICIO")
          await CargarCombosServicioAsync(model);
        return View(model);

      }

      var animalQ = await ScopeAnimalesAsync(_context.Animals);
      var animal = await animalQ.FirstOrDefaultAsync(a => a.Id == model.IdAnimal);

      if (animal == null)
        return Forbid();

      var hatoEvento = animal.idHato;
      var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
      int? userId = int.TryParse(userIdStr, out var u) ? u : null;

      var fechaEvt = model.FechaEvento!.Value.ToDateTime(TimeOnly.MinValue);

      using var tx = await _context.Database.BeginTransactionAsync();

      // si el usuario eligió estado productivo, lo actualizamos
      if (model.EstadoProductivoId != null)
      {
        animal.estadoProductivoId = model.EstadoProductivoId;
        _context.Update(animal);
      }

      switch (model.TipoEvento)
      {

        case "PARTO":
          {
            // ===== Validaciones básicas =====
            if (model.IdSexoCria == null)
              ModelState.AddModelError(nameof(model.IdSexoCria), "Seleccione el sexo de la cría.");

            if (model.IdTipoParto == null)
              ModelState.AddModelError(nameof(model.IdTipoParto), "Seleccione el tipo de parto.");

            if (model.IdEstadoCria == null)
              ModelState.AddModelError(nameof(model.IdEstadoCria), "Seleccione el estado de la cría.");

            if (model.HoraParto == null)
              ModelState.AddModelError(nameof(model.HoraParto), "Ingrese la hora del parto.");

            // Leer texto del sexo (para saber si es mellizo)
            var sexoTxt = "";
            if (model.IdSexoCria != null)
            {
              sexoTxt = await _context.SexoCria
                  .Where(x => x.Id == model.IdSexoCria.Value)
                  .Select(x => x.Nombre)
                  .FirstOrDefaultAsync() ?? "";
            }

            var t = sexoTxt.Trim().ToUpperInvariant();

            bool esMellizo = false, hembra1 = false, hembra2 = false;

            if (t.Contains("HEMBRA-HEMBRA")) { esMellizo = true; hembra1 = true; hembra2 = true; }
            else if (t.Contains("HEMBRA-MACHO")) { esMellizo = true; hembra1 = true; hembra2 = false; }
            else if (t.Contains("MACHO-HEMBRA")) { esMellizo = true; hembra1 = false; hembra2 = true; }
            else if (t.Contains("MACHO-MACHO")) { esMellizo = true; hembra1 = false; hembra2 = false; }
            else if (t.Contains("HEMBRA")) { esMellizo = false; hembra1 = true; hembra2 = false; }
            else if (t.Contains("MACHO")) { esMellizo = false; hembra1 = false; hembra2 = false; }

            // nombres obligatorios solo si quieres
            if (hembra1 && string.IsNullOrWhiteSpace(model.NombreCria1))
              ModelState.AddModelError(nameof(model.NombreCria1), "Ingrese el nombre de la cría 1.");

            // ===== Validar aretes y duplicados =====
            var arete1 = (model.AreteCria1 ?? "").Trim();


            if (!string.IsNullOrWhiteSpace(arete1))
            {
              var existeArete1 = await _context.Animals.AnyAsync(a => a.arete == arete1);
              if (existeArete1)
                ModelState.AddModelError(nameof(model.AreteCria1), "Ya existe un animal con ese Areté.");
            }



            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await CargarCombosPartoAsync(model);
              return View(model);
            }

            // ===== Fecha + hora del parto =====
            var fechaHoraParto = model.FechaEvento!.Value.ToDateTime(model.HoraParto!.Value);

            var ultimoPartoAnimal = await _context.Partos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { p.fechaRegistro, rr.idAnimal })
                .Where(x => x.idAnimal == animal.Id && x.fechaRegistro < fechaHoraParto)
                .OrderByDescending(x => x.fechaRegistro)
                .Select(x => (DateTime?)x.fechaRegistro)
                .FirstOrDefaultAsync();

            if (ultimoPartoAnimal != null)
            {
              var ultimaSecaAnimal = await _context.Secas.AsNoTracking()
                  .Join(_context.RegistroReproduccions.AsNoTracking(),
                      s => s.idRegistroReproduccion,
                      rr => rr.Id,
                      (s, rr) => new { s.fechaSeca, rr.idAnimal })
                  .Where(x => x.idAnimal == animal.Id && x.fechaSeca != null && x.fechaSeca.Value < fechaHoraParto)
                  .OrderByDescending(x => x.fechaSeca)
                  .Select(x => (DateTime?)x.fechaSeca)
                  .FirstOrDefaultAsync();

              if (ultimaSecaAnimal == null || ultimaSecaAnimal.Value < ultimoPartoAnimal.Value)
              {
                ModelState.AddModelError("", "No se puede registrar PARTO: la campaña anterior no está cerrada. Registra SECA para terminar la campaña antes de iniciar otra.");
                await tx.RollbackAsync();
                await CargarCombosAsync(model);
                await CargarCombosPartoAsync(model);
                return View(model);
              }
            }

            // Tipo parto texto
            var tipoPartoNombre = await _context.TipoPartos
                .Where(x => x.Id == model.IdTipoParto!.Value)
                .Select(x => x.Nombre)
                .FirstOrDefaultAsync() ?? "PARTO";

            // ===== Ciclo activo (preñez confirmada) =====
            var ciclo = await (
                from p in _context.Prenezs
                join c in _context.ConfirmacionPrenezs on p.idRegistroReproduccion equals c.idRegistroReproduccion
                where p.idMadreAnimal == animal.Id
                      && c.tipo == "POSITIVA"
                      && !_context.Partos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                      && !_context.Abortos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                orderby c.fechaRegistro descending
                select new { rrId = p.idRegistroReproduccion, padreId = p.idPadreAnimal }
            ).FirstOrDefaultAsync();

            if (ciclo == null)
            {
              ModelState.AddModelError("", "No se puede registrar PARTO: no hay preñez confirmada (POSITIVA) activa.");
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await CargarCombosPartoAsync(model);
              return View(model);
            }

            // exigir seca antes de parto
            var tieneSeca = await _context.Secas.AnyAsync(s => s.idRegistroReproduccion == ciclo.rrId);
            if (!tieneSeca)
            {
              ModelState.AddModelError("", "No se puede registrar PARTO: primero registra la SECA de este ciclo.");
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await CargarCombosPartoAsync(model);
              return View(model);
            }

            var rrIdCiclo = ciclo.rrId;
            var padreIdCiclo = ciclo.padreId;

            // Validación de fechas (ya la tienes)
            var errFechasParto = await ValidarFechaPartoVsEventosAsync(rrIdCiclo, fechaHoraParto);
            if (errFechasParto != null)
            {
              ModelState.AddModelError("", errFechasParto);
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await CargarCombosPartoAsync(model);
              return View(model);
            }

            // ===== PVE =====
            var pveDias = await GetPveDiasPorAnimalAsync(animal);  // devuelve 50–70 (o 60 default)
            var fechaFinPve = DateOnly.FromDateTime(fechaHoraParto).AddDays(pveDias);


            // ===== Guardar Parto =====
            var parto = new Parto
            {
              idHato = hatoEvento,
              idRegistroReproduccion = rrIdCiclo,
              tipo = tipoPartoNombre,
              fechaRegistro = fechaHoraParto,
              idSexoCria = model.IdSexoCria.Value,
              idTipoParto = model.IdTipoParto.Value,
              idEstadoCria = model.IdEstadoCria.Value,
              nombreCria1 = model.NombreCria1,
              areteCria1 = arete1,
              horaParto = model.HoraParto.Value,
              pveDias = pveDias,

              // SOLO si NO es computed:
              fechaFinPve = fechaFinPve

            };

            _context.Partos.Add(parto);
            await _context.SaveChangesAsync();

            // ===== Crear animales crías =====
            var idActivo = await _context.EstadoAnimals
                .Where(x => x.nombre == "ACTIVO")
                .Select(x => x.Id)
                .FirstAsync();

            var fechaNac = model.FechaEvento!.Value;

            var nombreCria1 = !string.IsNullOrWhiteSpace(model.NombreCria1)
                ? model.NombreCria1.Trim()
                : $"CRIA {arete1}";

            var cria1 = new Animal
            {
              nombre = nombreCria1,
              arete = arete1,
              codigo = arete1, // si tu BD exige codigo, usa el arete
              sexo = hembra1 ? "HEMBRA" : "MACHO",
              fechaNacimiento = fechaNac,
              idMadre = animal.Id,
              idPadre = padreIdCiclo,
              idHato = hatoEvento,
              idRaza = animal.idRaza,
              estadoId = idActivo,
              nacimientoEstimado = false
            };
            _context.Animals.Add(cria1);
            await _context.SaveChangesAsync();

            // ===== RegistroIngresos (ALTA) =====
            var fechaIngreso = DateOnly.FromDateTime(fechaHoraParto);
            var ahora = DateTime.Now;

            _context.RegistroIngresos.Add(new RegistroIngreso
            {
              codigoIngreso = $"ING-{ahora:yyyyMMddHHmmss}-{cria1.Id}",
              tipoIngreso = "ALTA",
              idAnimal = cria1.Id,
              fechaIngreso = fechaIngreso,
              idHato = cria1.idHato,
              usuarioId = userId,
              origen = "NACIMIENTO",
              observacion = "Alta automática por parto"
            });

            // ===== RegistroNacimiento =====
            _context.RegistroNacimientos.Add(new RegistroNacimiento
            {
              idAnimal = cria1.Id,
              idRegistroReproduccion = rrIdCiclo,
              fecha = fechaNac,
              observacionesNacimiento = "Nacimiento automático por PARTO"
            });

            var idLactando = await _context.EstadoProductivos
                .Where(e => e.nombre == "LACTANDO")
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();

            if (idLactando != null)
            {
              animal.estadoProductivoId = idLactando.Value;
              _context.Animals.Update(animal);
            }

            await _context.SaveChangesAsync();
            break;
          }



        case "CELO":
          {
            var rr = new RegistroReproduccion
            {
              idAnimal = animal.Id,
              fechaRegistro = fechaEvt,
              idHato = hatoEvento
            };
            _context.RegistroReproduccions.Add(rr);
            await _context.SaveChangesAsync();

            _context.Prenezs.Add(new Prenez
            {
              idRegistroReproduccion = rr.Id,
              idHato = hatoEvento,
              fechaCelo = model.FechaEvento.Value,
              idMadreAnimal = animal.Id,
              idPadreAnimal = null,
              fechaInseminacion = null,
              observacion = model.Observaciones
            });

            await _context.SaveChangesAsync();
            break;
          }

        case "SERVICIO":
          {
            const int DIAS_GESTACION = 279;
            const int DIAS_SECA_ANTES_PARTO = 60;

            var DIAS_PVE = await GetPveDiasPorAnimalAsync(animal);

            // Último PARTO (para PVE)
            var fechaUltimoParto = await _context.Partos
                .Include(p => p.idRegistroReproduccionNavigation)
                .Where(p => p.idRegistroReproduccionNavigation.idAnimal == animal.Id)
                .MaxAsync(p => (DateTime?)p.fechaRegistro);

            if (fechaUltimoParto != null)
            {
              var fechaMin = fechaUltimoParto.Value.Date.AddDays(DIAS_PVE);
              if (fechaEvt.Date < fechaMin)
                ModelState.AddModelError("", $"Debe respetar el P.V.E de {DIAS_PVE} días después del PARTO. Fecha mínima para inseminar: {fechaMin:dd/MM/yyyy}.");
            }

            // No permitir fecha anterior al último servicio
            DateOnly? fechaUltimoServicioDate = await _context.Prenezs
                .Where(p => p.idMadreAnimal == animal.Id && p.fechaInseminacion != null)
                .MaxAsync(p => p.fechaInseminacion);

            DateTime? fechaUltimoServicio = fechaUltimoServicioDate.HasValue
                ? fechaUltimoServicioDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            if (fechaUltimoServicio != null && fechaEvt < fechaUltimoServicio.Value)
              ModelState.AddModelError("", "No se permiten inseminaciones con fecha anterior a la última inseminación registrada.");

            // Estado SECA no puede inseminar
            var idSeca = await _context.EstadoProductivos
                .Where(e => e.nombre == "SECA")
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();

            if (idSeca != null && animal.estadoProductivoId == idSeca)
              ModelState.AddModelError("", "No se puede inseminar un animal en estado SECA.");

            // Validar servicio anterior (debe estar confirmado y no positivo)
            var lastServ = await _context.Prenezs
                .Where(p => p.idMadreAnimal == animal.Id && p.fechaInseminacion != null)
                .OrderByDescending(p => p.fechaInseminacion)
                .Select(p => new { rrId = p.idRegistroReproduccion })
                .FirstOrDefaultAsync();

            if (lastServ != null)
            {
              var lastConf = await _context.ConfirmacionPrenezs
                  .Where(c => c.idRegistroReproduccion == lastServ.rrId)
                  .OrderByDescending(c => c.fechaRegistro)
                  .Select(c => new { c.tipo })
                  .FirstOrDefaultAsync();

              if (lastConf == null)
                ModelState.AddModelError("", "No se puede registrar servicio: existe un servicio anterior sin confirmación de preñez.");
              else if (lastConf.tipo == "POSITIVA")
                ModelState.AddModelError("", "No se puede registrar servicio: el animal tiene preñez confirmada POSITIVA activa.");
            }

            // Autocompletar toro si eligió uno
            if (model.IdPadreAnimal.HasValue)
            {
              var torosScope = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

              var toro = await torosScope
                  .Where(a => a.Id == model.IdPadreAnimal.Value)
                  .Select(a => new { a.nombre, a.naab, a.sexo })
                  .FirstOrDefaultAsync();

              if (toro == null)
                ModelState.AddModelError(nameof(model.IdPadreAnimal), "Toro no válido o sin acceso.");
              else if ((toro.sexo ?? "").Trim().ToUpper() != "MACHO")
                ModelState.AddModelError(nameof(model.IdPadreAnimal), "El animal seleccionado no es macho.");
              else
              {

                if (string.IsNullOrWhiteSpace(model.CodigoNaab)) model.CodigoNaab = toro.naab;
              }

            }

            // Validación MONTA vs IA
            if (model.TipoServicio == "MONTA")
            {
              if (model.IdPadreAnimal == null)
                ModelState.AddModelError(nameof(model.IdPadreAnimal), "Seleccione el toro para Monta Natural.");

              model.CodigoNaab = null;
              model.Protocolo = null;

              model.HoraServicio = null;
            }
            else // IA (incluye null -> lo tratas como IA)
            {
              if (model.HoraServicio == null)
                ModelState.AddModelError(nameof(model.HoraServicio), "Ingrese hora de servicio.");

              if (string.IsNullOrWhiteSpace(model.Protocolo))
                ModelState.AddModelError(nameof(model.Protocolo), "Seleccione un protocolo.");

              if (string.IsNullOrWhiteSpace(model.CodigoNaab))
                ModelState.AddModelError(nameof(model.CodigoNaab), "Ingrese NAAB.");


            }

            // ✅ AHORA SÍ: cortar antes de guardar
            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await CargarCombosServicioAsync(model);
              return View(model);
            }

            // Guardar RR
            var rr = new RegistroReproduccion
            {
              idAnimal = animal.Id,
              fechaRegistro = fechaEvt,
              idHato = hatoEvento
            };

            _context.RegistroReproduccions.Add(rr);
            await _context.SaveChangesAsync();

            // Fechas
            var fechaInsem = model.FechaEvento!.Value;
            var fechaProbParto = fechaInsem.AddDays(DIAS_GESTACION);
            var fechaProbSeca = fechaProbParto.AddDays(-DIAS_SECA_ANTES_PARTO);

            var nroServ = await _context.Prenezs
                .CountAsync(p => p.idMadreAnimal == animal.Id && p.fechaInseminacion != null) + 1;
            var prenez = new Prenez
            {
              idRegistroReproduccion = rr.Id,
              idHato = hatoEvento,
              idMadreAnimal = animal.Id,
              idPadreAnimal = model.IdPadreAnimal,

              fechaInseminacion = fechaInsem,
              horaServicio = model.HoraServicio,
              numeroServicio = nroServ,
              codigoNaab = model.CodigoNaab,
              protocolo = model.Protocolo,

              fechaProbableParto = fechaProbParto,
              fechaProbableSeca = fechaProbSeca,

              observacion = model.Observaciones,
            };
            if (model.IdInseminador != null)
            {
              prenez.IdInseminador = model.IdInseminador;
            }
            _context.Prenezs.Add(prenez);

            await _context.SaveChangesAsync();
            break;
          }



        case "CONFIRMACION_PREÑEZ":
          {
            if (string.IsNullOrWhiteSpace(model.ConfirmacionTipo))
              ModelState.AddModelError(nameof(model.ConfirmacionTipo), "Seleccione POSITIVA o NEGATIVA.");

            if (string.IsNullOrWhiteSpace(model.ConfirmacionMetodo))
              ModelState.AddModelError(nameof(model.ConfirmacionMetodo), "Seleccione el método de confirmación.");

            // Último servicio (inseminación)
            var lastServicio = await _context.Prenezs
            .AsNoTracking()
            .Where(p => p.idMadreAnimal == animal.Id && p.fechaInseminacion != null)
            .Where(p => !_context.ConfirmacionPrenezs.Any(c => c.idRegistroReproduccion == p.idRegistroReproduccion))
            .OrderByDescending(p => p.fechaInseminacion)
            .Select(p => new { p.idRegistroReproduccion, p.fechaInseminacion })
            .FirstOrDefaultAsync();


            if (lastServicio == null)
              ModelState.AddModelError("", "No se puede confirmar preñez: el animal no tiene inseminación registrada.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            // Último servicio

            if (lastServicio == null)
              ModelState.AddModelError("", "No se puede confirmar preñez: el animal no tiene inseminación registrada.");

            var yaConfirmado = lastServicio != null && await _context.ConfirmacionPrenezs
                .AnyAsync(c => c.idRegistroReproduccion == lastServicio.idRegistroReproduccion);

            if (yaConfirmado)
              ModelState.AddModelError("", "Este servicio ya tiene confirmación registrada. (Solo se permite 1 confirmación por servicio).");

            int minDias = 35;
            if (lastServicio != null)
            {
              var fechaMin = lastServicio.fechaInseminacion!.Value.AddDays(minDias);
              if (model.FechaEvento!.Value < fechaMin)
                ModelState.AddModelError("", $"No se puede confirmar preñez antes de {minDias} días. Fecha mínima: {fechaMin:dd/MM/yyyy}.");
            }

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            _context.ConfirmacionPrenezs.Add(new ConfirmacionPrenez
            {
              idHato = hatoEvento,

              tipo = model.ConfirmacionTipo!,
              metodo = model.ConfirmacionMetodo,          // <-- NUEVO
              fechaRegistro = fechaEvt,
              idRegistroReproduccion = lastServicio.idRegistroReproduccion,
              observacion = model.Observaciones
            });

            await _context.SaveChangesAsync();
            break;
          }


        case "SECA":
          {
            // ✅ Regla de campañas por animal:
            // La SECA cierra una campaña (lactancia). Debe existir un PARTO previo y no debe haberse cerrado ya.
            var ultimoPartoAnimal = await _context.Partos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { p.fechaRegistro, rr.idAnimal })
                .Where(x => x.idAnimal == animal.Id && x.fechaRegistro <= fechaEvt)
                .OrderByDescending(x => x.fechaRegistro)
                .Select(x => (DateTime?)x.fechaRegistro)
                .FirstOrDefaultAsync();

            if (ultimoPartoAnimal == null)
              ModelState.AddModelError("", "No se puede registrar SECA: el animal no tiene PARTO previo (no hay campaña que cerrar).");
            else
            {
              var ultimaSecaAnimal = await _context.Secas.AsNoTracking()
                  .Join(_context.RegistroReproduccions.AsNoTracking(),
                      s => s.idRegistroReproduccion,
                      rr => rr.Id,
                      (s, rr) => new { s.fechaSeca, rr.idAnimal })
                  .Where(x => x.idAnimal == animal.Id && x.fechaSeca != null && x.fechaSeca.Value <= fechaEvt)
                  .OrderByDescending(x => x.fechaSeca)
                  .Select(x => (DateTime?)x.fechaSeca)
                  .FirstOrDefaultAsync();

              if (ultimaSecaAnimal != null && ultimaSecaAnimal.Value > ultimoPartoAnimal.Value)
                ModelState.AddModelError("", $"No se puede registrar SECA: la campaña ya fue cerrada el {ultimaSecaAnimal.Value:dd/MM/yyyy}.");

              if (fechaEvt <= ultimoPartoAnimal.Value)
                ModelState.AddModelError(nameof(model.FechaEvento), $"La SECA debe ser posterior al PARTO ({ultimoPartoAnimal.Value:dd/MM/yyyy}).");
            }

            // ✅ cortar si hubo errores ANTES de seguir con tu lógica de ciclo
            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            // Buscar ciclo activo: servicio con confirmación POSITIVA y sin PARTO/ABORTO
            var ciclo = await (
                 from p in _context.Prenezs
                 join c in _context.ConfirmacionPrenezs on p.idRegistroReproduccion equals c.idRegistroReproduccion
                 where p.idMadreAnimal == animal.Id
                       && c.tipo == "POSITIVA"
                       && !_context.Partos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                       && !_context.Abortos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                 orderby c.fechaRegistro descending
                 select new { rrId = p.idRegistroReproduccion, p.fechaInseminacion, p.fechaProbableParto }
             ).FirstOrDefaultAsync();

            if (ciclo == null)
              ModelState.AddModelError("", "No se puede registrar SECA: no hay preñez confirmada (POSITIVA) activa.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            var fInsem = ciclo!.fechaInseminacion!.Value;
            var fProb = ciclo.fechaProbableParto ?? fInsem.AddDays(279);
            var fSecaCalc = fProb.AddDays(-60);

            if (model.FechaEvento!.Value != fSecaCalc)
              ModelState.AddModelError(nameof(model.FechaEvento), $"La SECA debe ser 60 días antes del parto. Fecha requerida: {fSecaCalc:dd/MM/yyyy}.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            // ✅ Marcar estado productivo = SECA
            var idEstadoSeca = await _context.EstadoProductivos
                .Where(e => e.nombre == "SECA")
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();

            if (idEstadoSeca == null)
            {
              ModelState.AddModelError("", "No existe el Estado Productivo 'SECA' en la tabla EstadoProductivo.");
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            animal.estadoProductivoId = idEstadoSeca.Value;
            // (animal está trackeado, pero esto lo deja explícito)
            _context.Animals.Update(animal);


            _context.Secas.Add(new Seca
            {
              idHato = hatoEvento,
              idRegistroReproduccion = ciclo!.rrId,  // <-- MISMO RR DEL SERVICIO
              fechaSeca = fechaEvt,                 // <-- NUEVO
              motivo = model.Observaciones
            });

            await _context.SaveChangesAsync();
            break;
          }


        case "ABORTO":
          {
            if (model.IdCausaAborto == null)
              ModelState.AddModelError(nameof(model.IdCausaAborto), "Seleccione la causa del aborto.");

            var ciclo = await (
                from p in _context.Prenezs
                join c in _context.ConfirmacionPrenezs on p.idRegistroReproduccion equals c.idRegistroReproduccion
                where p.idMadreAnimal == animal.Id
                      && c.tipo == "POSITIVA"
                      && !_context.Partos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                      && !_context.Abortos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                orderby c.fechaRegistro descending
                select new { rrId = p.idRegistroReproduccion, fInsem = p.fechaInseminacion }
            ).FirstOrDefaultAsync();

            if (ciclo == null)
              ModelState.AddModelError("", "No se puede registrar aborto: no hay preñez confirmada (POSITIVA) activa.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            var fInsem = ciclo!.fInsem!.Value;
            var fAborto = model.FechaEvento!.Value;

            if (fAborto < fInsem)
              ModelState.AddModelError(nameof(model.FechaEvento), "La fecha de aborto no puede ser anterior a la inseminación.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            int diasATermino = fAborto.DayNumber - fInsem.DayNumber;

            var aborto = new Aborto
            {
              idHato = hatoEvento,
              idRegistroReproduccion = ciclo.rrId,  // <-- MISMO RR DEL SERVICIO
              idCausaAborto = model.IdCausaAborto.Value,
              fechaRegistro = fechaEvt,
              diasATermino = diasATermino           // <-- NUEVO
            };

            _context.Abortos.Add(aborto);
            await _context.SaveChangesAsync();
            break;
          }

        case "VENTA":
        case "MUERTE":
          {
            if (model.TipoEvento == "VENTA" && string.IsNullOrWhiteSpace(model.DestinoSalida))
              ModelState.AddModelError(nameof(model.DestinoSalida), "Ingrese el comprador/destino de la venta.");

            if (model.TipoEvento == "MUERTE" &&
                string.IsNullOrWhiteSpace(model.DestinoSalida) &&
                string.IsNullOrWhiteSpace(model.Observaciones))
              ModelState.AddModelError(nameof(model.DestinoSalida), "Ingrese la causa de muerte o una observación.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            _context.RegistroSalida.Add(new RegistroSalidum
            {
              nombre = model.DestinoSalida ?? "-",
              tipoSalida = model.TipoEvento,
              idAnimal = animal.Id,
              fechaSalida = model.FechaEvento.Value,
              idHato = hatoEvento,
              usuarioId = userId,
              destino = model.DestinoSalida,
              observacion = model.Observaciones
            });

            var inactivoId = await _context.EstadoAnimals
                .Where(x => x.nombre == "INACTIVO")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            if (inactivoId == null)
            {
              ModelState.AddModelError("", "No existe el Estado Animal 'INACTIVO'.");
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            animal.estadoId = inactivoId.Value;
            _context.Update(animal);

            await _context.SaveChangesAsync();
            break;
          }




        /*case "ENFERMEDAD":
          {
            if (model.IdTipoEnfermedad == null)
              ModelState.AddModelError(nameof(model.IdTipoEnfermedad), "Seleccione tipo de enfermedad.");
            if (model.IdVeterinario == null)
              ModelState.AddModelError(nameof(model.IdVeterinario), "Seleccione veterinario.");

            if (!ModelState.IsValid) { await tx.RollbackAsync(); await CargarCombosAsync(model); return View(model); }

            _context.Enfermedads.Add(new Enfermedad
            {
              fechaDiagnostico = fechaEvt,
              fechaRecuperacion = null,
              idVeterinario = model.IdVeterinario!.Value,
              idTipoEnfermedad = model.IdTipoEnfermedad!.Value,
              idAnimal = animal.Id
            });

            await _context.SaveChangesAsync();
            break;
          }*/

        /*case "MEDICACION":
          {
            if (model.IdAnimal == null)
              ModelState.AddModelError(nameof(model.IdAnimal), "Seleccione un animal.");

            if (model.IdEnfermedad == null)
              ModelState.AddModelError(nameof(model.IdEnfermedad), "Seleccione el caso (enfermedad).");

            if (model.IdTipoTratamiento == null)
              ModelState.AddModelError(nameof(model.IdTipoTratamiento), "Seleccione el tratamiento.");

            if (model.FechaEvento == null)
              ModelState.AddModelError(nameof(model.FechaEvento), "Seleccione la fecha del evento.");

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await PrepararMedicacionAsync(model);
              return View(model);

            }

            // model.FechaEvento ES DateOnly? -> convertir a DateTime (datetime2)
            var fechaInicioDt = model.FechaEvento.Value.ToDateTime(TimeOnly.MinValue);

            var enf = await _context.Enfermedads
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == model.IdEnfermedad.Value
                                       && e.idAnimal == model.IdAnimal.Value);

            if (enf == null)
            {
              ModelState.AddModelError(nameof(model.IdEnfermedad), "El caso seleccionado no pertenece al animal.");
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              await PrepararMedicacionAsync(model);
              return View(model);
            }

            var trat = new Tratamiento
            {
              idEnfermedad = enf.Id,
              idTipoTratamiento = model.IdTipoTratamiento.Value,
              fechaInicio = fechaInicioDt,
              fechaFinalEstimada = null,
              costoEstimado = model.CostoEstimado,
              observaciones = model.Observaciones
            };

            _context.Tratamientos.Add(trat);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            TempData["EventosMessage"] = "Tratamiento registrado.";
            return RedirectToAction(nameof(Registrar));
          }*/

        default:
          {
            // Validaciones por tipo
            if (model.TipoEvento == "ANALISIS")
            {
              if (string.IsNullOrWhiteSpace(model.TipoAnalisis))
                ModelState.AddModelError(nameof(model.TipoAnalisis), "Ingrese el tipo de análisis.");

              if (string.IsNullOrWhiteSpace(model.Resultado))
                ModelState.AddModelError(nameof(model.Resultado), "Ingrese el resultado.");
            }

            // Para RECHAZO / INDICACION_ESPECIAL, al menos algo de texto
            if ((model.TipoEvento == "RECHAZO" || model.TipoEvento == "INDICACION_ESPECIAL") &&
                string.IsNullOrWhiteSpace(model.Observaciones))
            {
              ModelState.AddModelError(nameof(model.Observaciones), "Ingrese una descripción / observación.");
            }

            if (!ModelState.IsValid)
            {
              await tx.RollbackAsync();
              await CargarCombosAsync(model);
              return View(model);
            }

            _context.EventoGenerals.Add(new EventoGeneral
            {
              idAnimal = animal.Id,
              idHato = hatoEvento,
              fechaEvento = fechaEvt,
              tipoEvento = model.TipoEvento,
              tipoAnalisis = model.TipoEvento == "ANALISIS" ? model.TipoAnalisis : null,
              resultado = model.TipoEvento == "ANALISIS" ? model.Resultado : null,
              descripcion = model.Observaciones,
              usuarioId = userId
            });

            await _context.SaveChangesAsync();
            break;
          }

      }

      await tx.CommitAsync();

      TempData["EventosMessage"] = "Evento registrado correctamente.";
      return RedirectToAction(nameof(Registrar));
    }

    [HttpGet]
    public async Task<IActionResult> DatosAnimal(int idAnimal)
    {
      var q = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());

      var data = await q
          .Where(a => a.Id == idAnimal)
          .Select(a => new
          {
            arete = a.arete,
            rp = a.codigo,
            nombre = a.nombre
          })
          .FirstOrDefaultAsync();

      if (data == null) return Json(new { arete = "", rp = "", nombre = "" });

      return Json(data);
    }


    private async Task CargarEnfermedadesPorAnimalAsync(int? idAnimal)
    {
      // si no hay animal, combos vacíos
      if (idAnimal == null)
      {
        ViewBag.IdEnfermedad = new SelectList(Enumerable.Empty<SelectListItem>());
        ViewBag.IdTipoTratamiento = new SelectList(Enumerable.Empty<SelectListItem>());
        return;
      }

      // casos (enfermedad) abiertos del animal
      var casos = await _context.Enfermedads
          .AsNoTracking()
          .Include(e => e.idTipoEnfermedadNavigation)
          .Where(e => e.idAnimal == idAnimal.Value && e.fechaRecuperacion == null)
          .OrderByDescending(e => e.fechaDiagnostico)
          .Select(e => new
          {
            e.Id,
            Texto = $"{e.idTipoEnfermedadNavigation.nombre} ({e.fechaDiagnostico:dd/MM/yyyy})"
          })
          .ToListAsync();

      ViewBag.IdEnfermedad = new SelectList(casos, "Id", "Texto");
      ViewBag.IdTipoTratamiento = new SelectList(Enumerable.Empty<SelectListItem>());
    }


    private async Task CargarCombosPartoAsync(RegistrarEventoViewModel? model = null)
    {
      ViewBag.IdSexoCria = new SelectList(
          await _context.SexoCria.OrderBy(x => x.Nombre).ToListAsync(),
          "Id", "Nombre", model?.IdSexoCria
      );

      ViewBag.IdTipoParto = new SelectList(
          await _context.TipoPartos.OrderBy(x => x.Nombre).ToListAsync(),
          "Id", "Nombre", model?.IdTipoParto
      );

      ViewBag.IdEstadoCria = new SelectList(
          await _context.EstadoCria.OrderBy(x => x.Nombre).ToListAsync(),
          "Id", "Nombre", model?.IdEstadoCria
      );
    }

    private static int GetMinDiasConfirmacionPrenez(string? metodo)
    {
      // RQ: 35 días ecografía / 60 días palpación
      return (metodo ?? "").Trim().ToUpperInvariant() switch
      {
        "ECOGRAFIA" => 35,
        "PALPACION" => 60,
        _ => 35 // por defecto alineado al RQ (más conservador)
      };
    }

    private async Task<string?> ValidarFechaPartoVsEventosAsync(int idRegistroReproduccion, DateTime fechaHoraParto)
    {
      // 1) Inseminación (fecha + hora servicio si existe)
      var servicio = await _context.Prenezs
          .AsNoTracking()
          .Where(p => p.idRegistroReproduccion == idRegistroReproduccion)
          .Select(p => new { p.fechaInseminacion, p.horaServicio })
          .FirstOrDefaultAsync();

      if (servicio?.fechaInseminacion != null)
      {
        var hora = servicio.horaServicio ?? new TimeOnly(0, 0);
        var fechaHoraInsem = servicio.fechaInseminacion.Value.ToDateTime(hora);

        if (fechaHoraParto < fechaHoraInsem)
          return $"La fecha/hora de PARTO ({fechaHoraParto:dd/MM/yyyy HH:mm}) no puede ser anterior a la INSEMINACIÓN ({fechaHoraInsem:dd/MM/yyyy HH:mm}).";
      }

      // 2) Confirmación POSITIVA (última del ciclo)
      var fechaConfPos = await _context.ConfirmacionPrenezs
          .AsNoTracking()
          .Where(c => c.idRegistroReproduccion == idRegistroReproduccion && c.tipo == "POSITIVA")
          .OrderByDescending(c => c.fechaRegistro)
          .Select(c => (DateTime?)c.fechaRegistro)
          .FirstOrDefaultAsync();

      if (fechaConfPos != null && fechaHoraParto < fechaConfPos.Value)
        return $"La fecha/hora de PARTO ({fechaHoraParto:dd/MM/yyyy HH:mm}) no puede ser anterior a la CONFIRMACIÓN DE PREÑEZ ({fechaConfPos:dd/MM/yyyy HH:mm}).";

      // 3) Seca (del ciclo)
      var fechaSeca = await _context.Secas
          .AsNoTracking()
          .Where(s => s.idRegistroReproduccion == idRegistroReproduccion)
          .OrderByDescending(s => s.fechaSeca)
          .Select(s => (DateTime?)s.fechaSeca)
          .FirstOrDefaultAsync();

      if (fechaSeca != null && fechaHoraParto < fechaSeca.Value)
        return $"La fecha/hora de PARTO ({fechaHoraParto:dd/MM/yyyy HH:mm}) no puede ser anterior a la SECA ({fechaSeca:dd/MM/yyyy HH:mm}).";

      return null;
    }
    private async Task<int> GetPveDiasPorAnimalAsync(Animal animal)
    {
      // 1) Priorizar el PVE del ÚLTIMO PARTO del animal (50/70)
      var pveUltimoParto = await (
          from p in _context.Partos.AsNoTracking()
          join rr in _context.RegistroReproduccions.AsNoTracking()
              on p.idRegistroReproduccion equals rr.Id
          where rr.idAnimal == animal.Id
          orderby p.fechaRegistro descending
          select (int?)p.pveDias
      ).FirstOrDefaultAsync();

      if (pveUltimoParto.HasValue && pveUltimoParto.Value > 0)
        return pveUltimoParto.Value;

      // 2) Si no hay PARTO (o no guardó pveDias), fallback al establo
      var establoId = await _context.Hatos
          .Where(h => h.Id == animal.idHato)
          .Select(h => h.EstabloId)
          .FirstOrDefaultAsync();

      var pveEstablo = await _context.Establos
          .Where(e => e.Id == establoId)
          .Select(e => (int?)e.pveDias)
          .FirstOrDefaultAsync();

      return pveEstablo ?? 60;
    }


    [HttpGet]
    public async Task<IActionResult> AnimalesPorEvento(string tipoEvento, string? fechaEvento)
    {
      // parse fecha (yyyy-MM-dd)
      DateOnly fecha = DateOnly.FromDateTime(DateTime.Today);
      if (!string.IsNullOrWhiteSpace(fechaEvento) && DateOnly.TryParse(fechaEvento, out var f))
        fecha = f;

      var animalesQ = await ScopeAnimalesAsync(_context.Animals.AsNoTracking());
      // SOLO HEMBRAS
      animalesQ = animalesQ.Where(a => a.sexo != null && a.sexo.Trim().ToUpper() == "HEMBRA");

      var tipo = (tipoEvento ?? "").Trim().ToUpperInvariant();

      if (tipo == "CONFIRMACION_PREÑEZ")
      {
        // Si quieres “elegibles” de verdad: ya pasaron 35 días desde la inseminación
        var limite = fecha.AddDays(-35);

        var madresPendientesQ =
            from p in _context.Prenezs.AsNoTracking()
            where p.fechaInseminacion != null
                  && p.fechaInseminacion <= limite
                  && !_context.ConfirmacionPrenezs.AsNoTracking()
                        .Any(c => c.idRegistroReproduccion == p.idRegistroReproduccion)
            select p.idMadreAnimal;

        animalesQ = animalesQ.Where(a => madresPendientesQ.Contains(a.Id));
      }

      if (tipo == "SERVICIO")
      {
        // 1) Excluir animales en estado productivo SECA
        var idSeca = await _context.EstadoProductivos.AsNoTracking()
            .Where(e => e.nombre == "SECA")
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();

        if (idSeca != null)
          animalesQ = animalesQ.Where(a => a.estadoProductivoId != idSeca.Value);

        var fechaDt = fecha.ToDateTime(TimeOnly.MinValue);

        // 2) PVE: si tiene último PARTO, debe haber terminado PVE (fechaFinPve <= fecha)
        var ultimoPartoPorAnimal =
            from p in _context.Partos.AsNoTracking()
            join rr in _context.RegistroReproduccions.AsNoTracking()
                on p.idRegistroReproduccion equals rr.Id
            group new { p, rr } by rr.idAnimal into g
            select new
            {
              idAnimal = g.Key,
              maxFecha = g.Max(x => x.p.fechaRegistro)
            };

        var finPvePorAnimal =
            from p in _context.Partos.AsNoTracking()
            join rr in _context.RegistroReproduccions.AsNoTracking()
                on p.idRegistroReproduccion equals rr.Id
            join m in ultimoPartoPorAnimal
                on new { idAnimal = rr.idAnimal, fecha = p.fechaRegistro }
                equals new { idAnimal = m.idAnimal, fecha = m.maxFecha }
            select new
            {
              idAnimal = m.idAnimal,
              finPveDt = p.fechaRegistro.Date.AddDays(p.pveDias ?? 60)
            };

        var idsConParto = finPvePorAnimal.Select(x => x.idAnimal);

        var idsPveOk = finPvePorAnimal
            .Where(x => x.finPveDt <= fechaDt)
            .Select(x => x.idAnimal);

        animalesQ = animalesQ.Where(a => !idsConParto.Contains(a.Id) || idsPveOk.Contains(a.Id));


        // 3) Servicio anterior: si existe, DEBE tener confirmación NEGATIVA (si no tiene confirmación, NO es elegible; si POSITIVA, NO es elegible)
        var ultimoServFechaPorAnimal =
            from p in _context.Prenezs.AsNoTracking()
            where p.idMadreAnimal != null && p.fechaInseminacion != null
            group p by p.idMadreAnimal.Value into g
            select new
            {
              idAnimal = g.Key,
              maxFecha = g.Max(x => x.fechaInseminacion)
            };

        var ultimoServRrPorAnimal =
            from p in _context.Prenezs.AsNoTracking()
            join m in ultimoServFechaPorAnimal
                on new { idAnimal = p.idMadreAnimal!.Value, fecha = p.fechaInseminacion }
                equals new { idAnimal = m.idAnimal, fecha = m.maxFecha }
            select new
            {
              m.idAnimal,
              rrId = p.idRegistroReproduccion
            };

        var ultimaConfFechaPorRr =
            from c in _context.ConfirmacionPrenezs.AsNoTracking()
            group c by c.idRegistroReproduccion into g
            select new
            {
              rrId = g.Key,
              maxFecha = g.Max(x => x.fechaRegistro)
            };

        var ultimaConfTipoPorRr =
            from c in _context.ConfirmacionPrenezs.AsNoTracking()
            join m in ultimaConfFechaPorRr
                on new { rrId = c.idRegistroReproduccion, fecha = c.fechaRegistro }
                equals new { rrId = m.rrId, fecha = m.maxFecha }
            select new
            {
              m.rrId,
              c.tipo
            };

        var idsConServicio = ultimoServRrPorAnimal.Select(x => x.idAnimal);

        // Elegibles por servicio: solo los que tienen confirmación NEGATIVA en su último servicio
        var idsServicioElegible =
            from s in ultimoServRrPorAnimal
            join c in ultimaConfTipoPorRr on s.rrId equals c.rrId
            where c.tipo == "NEGATIVA"
            select s.idAnimal;

        animalesQ = animalesQ.Where(a => !idsConServicio.Contains(a.Id) || idsServicioElegible.Contains(a.Id));
      }
      try
      {
        var animales = await animalesQ.CountAsync();
        var items = await animalesQ
            .OrderBy(a => a.arete)
            .Select(a => new
            {
              value = a.Id.ToString(),
              text = $"{(a.arete ?? "-")} - {a.nombre}"
            })
            .ToListAsync();

        return Json(items);
      }
      catch (SqlException ex)
      {
        /* NO HAY ANIMALES EN animalesQ */
        Console.WriteLine(ex.StackTrace);
        var items = new List<Dictionary<string, string>>();
        return Json(items);
      }
    }
  }
}
