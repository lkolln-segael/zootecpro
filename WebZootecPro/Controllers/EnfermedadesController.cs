using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Enfermedades;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,VETERINARIO")]
  public class EnfermedadesController : Controller
  {
    private readonly ZootecContext _context;

    public EnfermedadesController(ZootecContext context)
    {
      _context = context;
    }

    private int? GetUsuarioId()
    {
      var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.TryParse(s, out var id) ? id : (int?)null;
    }

    private async Task<Usuario?> GetUsuarioActualAsync()
    {
      var id = GetUsuarioId();
      if (id == null) return null;

      return await _context.Usuarios
          .AsNoTracking()
          .Include(u => u.Rol)
          .FirstOrDefaultAsync(u => u.Id == id.Value);
    }

    private async Task<int?> GetEstabloScopeAsync()
    {
      var empresa = await GetEmpresaAsync();
      var establo = await _context.Establos.FirstOrDefaultAsync(e => e.EmpresaId == empresa.Id);
      return establo.Id;
    }
    private async Task<Empresa?> GetEmpresaAsync()
    {
      var usuario = await GetUsuarioActualAsync();
      return await _context.Empresas.FirstOrDefaultAsync(e => e.usuarioID == usuario.Id
          || e.Colaboradors.Select(e => e.idUsuario).Contains(usuario.Id));
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var hoy = DateTime.Today;
      var establo = await GetEstabloScopeAsync();
      var enfermedades = await _context.Enfermedads
        .Where(e => e.idAnimalNavigation.idHatoNavigation.EstabloId == establo)
        .AsNoTracking()
        .ToListAsync();

      var model = enfermedades.Select(enf =>
      {
        var fin = (enf.fechaRecuperacion ?? hoy).Date;
        var dias = (fin - enf.fechaDiagnostico.Date).Days + 1;
        if (dias < 0) dias = 0;

        return new CrearEnfermedadViewModel
        {
          IdEnfermedad = enf.Id,
          FechaDiagnostico = enf.fechaDiagnostico,
          FechaRecuperacion = enf.fechaRecuperacion,
          IdVeterinario = enf.idVeterinario,
          IdTipoEnfermedad = enf.idTipoEnfermedad,
          IdAnimal = enf.idAnimal,
          DiasEnEnfermeria = dias,
          EnEnfermeria = enf.fechaRecuperacion == null
        };
      }).ToList();

      // combos/diccionarios
      ViewBag.IdAnimal = await _context.Animals
        .Where(a => a.idHatoNavigation.EstabloId == establo)
        .AsNoTracking()
        .ToDictionaryAsync(a => a.Id, a => a.codigo ?? a.nombre ?? "SIN CÓDIGO");

      ViewBag.IdTipoEnfermedad = await _context.TipoEnfermedades
        .AsNoTracking()
        .ToDictionaryAsync(t => t.Id, t => t.nombre);

      ViewBag.IdVeterinario = await _context.Usuarios
        .AsNoTracking()
        .Where(u => u.Rol.Nombre.ToUpper() == "VETERINARIO" && u.idEstablo == establo)
        .ToDictionaryAsync(v => v.Id, v => v.nombre);

      return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
      var establo = await GetEstabloScopeAsync();
      var model = new CrearEnfermedadViewModel
      {
        FechaDiagnostico = DateTime.Today,
        FechaRecuperacion = null, // ✅ por defecto queda en enfermería
        Animales = _context.Animals
          .Where(a => a.idHatoNavigation.EstabloId == establo)
          .Select(a => new SelectListItem
          {
            Value = a.Id.ToString(),
            Text = (a.codigo ?? "-") + " - " + (a.nombre ?? "-")
          }).ToList(),

        Veterinarios = _context.Usuarios
          .Where(v => v.idEstablo == establo && v.Rol.Nombre.ToUpper() == "VETERINARIO")
          .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.nombre })
          .ToList(),

        TipoEnfermedades = _context.TipoEnfermedades
          .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.nombre })
          .ToList()
      };

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CrearEnfermedadViewModel model)
    {
      ValidarFechasEnfermeria(model);

      if (!ModelState.IsValid)
      {
        RecargarSelects(model);
        return View(model);
      }

      var enfermedad = new Enfermedad
      {
        fechaDiagnostico = model.FechaDiagnostico.Date,
        fechaRecuperacion = model.FechaRecuperacion?.Date,
        idVeterinario = model.IdVeterinario,
        idTipoEnfermedad = model.IdTipoEnfermedad,
        idAnimal = model.IdAnimal
      };

      _context.Enfermedads.Add(enfermedad);
      _context.SaveChanges();

      return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var enf = await _context.Enfermedads.FindAsync(id);
      if (enf == null) return NotFound();

      var model = new CrearEnfermedadViewModel
      {
        IdEnfermedad = enf.Id,
        FechaDiagnostico = enf.fechaDiagnostico,
        FechaRecuperacion = enf.fechaRecuperacion,
        IdAnimal = enf.idAnimal,
        IdVeterinario = enf.idVeterinario,
        IdTipoEnfermedad = enf.idTipoEnfermedad,
      };

      RecargarSelects(model);
      ViewBag.Id = id;
      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CrearEnfermedadViewModel model)
    {
      ValidarFechasEnfermeria(model);

      if (!ModelState.IsValid)
      {
        RecargarSelects(model);
        ViewBag.Id = id;
        return View(model);
      }

      var enf = await _context.Enfermedads.FindAsync(id);
      if (enf == null) return NotFound();

      enf.fechaDiagnostico = model.FechaDiagnostico.Date;
      enf.fechaRecuperacion = model.FechaRecuperacion?.Date;
      enf.idAnimal = model.IdAnimal;
      enf.idVeterinario = model.IdVeterinario;
      enf.idTipoEnfermedad = model.IdTipoEnfermedad;

      _context.Update(enf);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Alta(int id)
    {
      var enf = await _context.Enfermedads.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
      if (enf == null) return NotFound();

      var vm = new AltaEnfermeriaViewModel
      {
        IdEnfermedad = id,
        FechaAlta = DateTime.Today
      };

      return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alta(AltaEnfermeriaViewModel vm)
    {
      if (!ModelState.IsValid) return View(vm);

      var enf = await _context.Enfermedads.FirstOrDefaultAsync(e => e.Id == vm.IdEnfermedad);
      if (enf == null) return NotFound();

      if (vm.FechaAlta.Date < enf.fechaDiagnostico.Date)
      {
        ModelState.AddModelError(nameof(vm.FechaAlta), "La fecha de alta no puede ser menor a la fecha de diagnóstico.");
        return View(vm);
      }

      enf.fechaRecuperacion = vm.FechaAlta.Date;
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    private void ValidarFechasEnfermeria(CrearEnfermedadViewModel model)
    {
      if (model.FechaRecuperacion.HasValue &&
          model.FechaRecuperacion.Value.Date < model.FechaDiagnostico.Date)
      {
        ModelState.AddModelError(nameof(model.FechaRecuperacion),
          "La fecha de recuperación no puede ser menor a la fecha de diagnóstico.");
      }
    }

    private void RecargarSelects(CrearEnfermedadViewModel model)
    {
      model.Animales = _context.Animals.Select(a => new SelectListItem
      {
        Value = a.Id.ToString(),
        Text = (a.codigo ?? "-") + " - " + (a.nombre ?? "-")
      }).ToList();

      model.Veterinarios = _context.Usuarios
        .Where(v => v.Rol.Nombre.ToUpper() == "VETERINARIO")
        .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.nombre })
        .ToList();

      model.TipoEnfermedades = _context.TipoEnfermedades
        .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.nombre })
        .ToList();
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
      var enfermedad = await _context.Enfermedads.FindAsync(id);
      if (enfermedad == null)
        return NotFound();

      var model = new CrearEnfermedadViewModel
      {
        FechaDiagnostico = enfermedad.fechaDiagnostico,
        FechaRecuperacion = enfermedad.fechaRecuperacion ?? new DateTime(),
        IdAnimal = enfermedad.idAnimal,
        IdVeterinario = enfermedad.idVeterinario,
        IdTipoEnfermedad = enfermedad.idTipoEnfermedad
      };

      ViewBag.Id = id;
      return View(model);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var enfermedad = await _context.Enfermedads.FindAsync(id);
      if (enfermedad == null)
        return NotFound();

      _context.Enfermedads.Remove(enfermedad);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

  }
}
