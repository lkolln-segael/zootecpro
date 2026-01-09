using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.CampaniaLechera;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
  public class CampaniasController : Controller
  {
    private readonly ZootecContext _context;

    public CampaniasController(ZootecContext context)
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
      var u = await GetUsuarioActualAsync();
      return u?.idEstablo;
    }

    public async Task<IActionResult> Index()
    {
      var establoId = await GetEstabloScopeAsync();

      IQueryable<CampaniaLechera> q = _context.CampaniaLecheras
.AsNoTracking()
.Include(c => c.Establo);

      if (establoId.HasValue)
        q = q.Where(c => c.EstabloId == establoId.Value);

      var data = await q.OrderByDescending(c => c.fechaInicio).ToListAsync();
      return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
      var establoId = await GetEstabloScopeAsync();

      ViewBag.Establos = await _context.Establos
          .AsNoTracking()
          .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.nombre })
          .ToListAsync();

      var m = new CampaniaLechera
      {
        fechaInicio = DateOnly.FromDateTime(DateTime.Now),
        fechaFin = DateOnly.FromDateTime(DateTime.Now),
        EstabloId = establoId ?? 0,
        activa = true
      };

      return View(m);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CampaniaLecheraModel model)
    {
      var campania = new CampaniaLechera()
      {
        EstabloId = model.EstabloId,
        nombre = model.nombre,
        activa = model.activa,
        fechaInicio = model.fechaInicio,
        fechaFin = model.fechaFin,
        observaciones = model.observaciones
      };
      if (model.fechaFin < model.fechaInicio)
        ModelState.AddModelError("fechaInicio", "La fecha fin no puede ser menor que la fecha inicio.");

      // Si el usuario está “amarrado” a establo, forzamos
      var establoScope = await GetEstabloScopeAsync();
      if (establoScope.HasValue)
        model.EstabloId = establoScope.Value;

      if (!ModelState.IsValid)
      {
        ViewBag.Establos = await _context.Establos.AsNoTracking()
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.nombre })
            .ToListAsync();
        return View(campania);
      }
      _context.CampaniaLecheras.Add(campania);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var item = await _context.CampaniaLecheras.FindAsync(id);
      if (item == null) return NotFound();

      var establoScope = await GetEstabloScopeAsync();
      if (establoScope.HasValue && item.EstabloId != establoScope.Value)
        return Forbid();

      ViewBag.Establos = await _context.Establos.AsNoTracking()
          .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.nombre })
          .ToListAsync();

      return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CampaniaLechera model)
    {
      if (id != model.Id) return BadRequest();

      if (model.fechaFin < model.fechaInicio)
        ModelState.AddModelError("", "La fecha fin no puede ser menor que la fecha inicio.");

      var establoScope = await GetEstabloScopeAsync();
      if (establoScope.HasValue)
        model.EstabloId = establoScope.Value;

      if (!ModelState.IsValid)
      {
        ViewBag.Establos = await _context.Establos.AsNoTracking()
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.nombre })
            .ToListAsync();
        return View(model);
      }

      _context.Entry(model).State = EntityState.Modified;
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }
  }
}
