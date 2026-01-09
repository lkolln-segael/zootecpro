using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Costos;

namespace WebZootecPro.Controllers
{
  public class CostosController : Controller
  {
    private readonly ZootecContext _context;
    public CostosController(ZootecContext context) => _context = context;

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
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, int? idEstablo, int? idHato, int? idAnimal, int? idTipoCosto, int? idCentroCosto)
    {
      var establoId = await GetEstabloScopeAsync();
      var q = _context.MovimientoCostos
          .Where(m => m.IdCentroCosto == establoId)
          .Include(x => x.IdCentroCostoNavigation)
          .Include(x => x.IdTipoCostoNavigation)
          .AsQueryable();

      if (desde.HasValue)
      {
        var d = DateOnly.FromDateTime(desde.Value.Date);
        q = q.Where(x => x.Fecha >= d);
      }
      if (hasta.HasValue)
      {
        var h = DateOnly.FromDateTime(hasta.Value.Date);
        q = q.Where(x => x.Fecha <= h);
      }
      if (idEstablo.HasValue) q = q.Where(x => x.IdEstablo == idEstablo);
      if (idHato.HasValue) q = q.Where(x => x.IdCorral == idHato);
      if (idAnimal.HasValue) q = q.Where(x => x.IdAnimal == idAnimal);
      if (idTipoCosto.HasValue) q = q.Where(x => x.IdTipoCosto == idTipoCosto);
      if (idCentroCosto.HasValue) q = q.Where(x => x.IdCentroCosto == idCentroCosto);

      var items = await q.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.IdMovimientoCosto).ToListAsync();

      var vm = new CostosIndexVm
      {
        Desde = desde,
        Hasta = hasta,
        IdEstablo = idEstablo,
        IdHato = idHato,
        IdAnimal = idAnimal,
        IdTipoCosto = idTipoCosto,
        IdCentroCosto = idCentroCosto,
        Items = items,
        Total = items.Sum(x => x.MontoTotal)
      };

      // combos para filtros (opcional: puedes moverlos a ViewBag si prefieres)
      ViewBag.TiposCosto = await _context.TipoCostos
          .OrderBy(t => t.Nombre)
          .Select(t => new SelectListItem { Value = t.IdTipoCosto.ToString(), Text = t.Nombre })
          .ToListAsync();

      ViewBag.CentrosCosto = await _context.CentroCostos
          .Where(c => c.Activo)
          .OrderBy(c => c.Nombre)
          .Select(c => new SelectListItem { Value = c.IdCentroCosto.ToString(), Text = c.Nombre })
          .ToListAsync();

      ViewBag.Establos = await _context.Establos
          .OrderBy(e => e.nombre)
          .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.nombre })
          .ToListAsync();

      ViewBag.Hatos = await _context.Hatos
          .OrderBy(h2 => h2.nombre)
          .Select(h2 => new SelectListItem { Value = h2.Id.ToString(), Text = h2.nombre })
          .ToListAsync();

      ViewBag.Animales = await _context.Animals
          .OrderBy(a => a.codigo)
          .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.codigo} - {a.nombre}" })
          .ToListAsync();

      return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
      var vm = new MovimientoCostoVm();
      ViewBag.Producciones = _context.RegistroProduccionLeches
      .Include(r => r.idAnimalNavigation)
      .Include(r => r.Calidads)
      .AsQueryable();

      await CargarCombos(vm);
      return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovimientoCostoVm vm)
    {
      if (!ModelState.IsValid)
      {
        await CargarCombos(vm);
        return View(vm);
      }

      // Auto-derivar Establo/Hato desde Animal si solo te pasan Animal
      if (vm.IdAnimal.HasValue)
      {
        var animal = await _context.Animals
            .Include(a => a.idHatoNavigation)
            .ThenInclude(h => h.Establo)
            .FirstOrDefaultAsync(a => a.Id == vm.IdAnimal.Value);

        if (animal != null)
        {
          vm.IdCorral ??= animal.idHato; // HatoId
          vm.IdEstablo ??= animal.idHatoNavigation.EstabloId;
        }
      }

      var entity = new MovimientoCosto
      {
        Fecha = DateOnly.FromDateTime(vm.Fecha.Date),
        IdCentroCosto = vm.IdCentroCosto,
        IdTipoCosto = vm.IdTipoCosto,
        MontoTotal = vm.MontoTotal,
        Descripcion = vm.Descripcion,
        IdEstablo = vm.IdEstablo,
        IdCorral = vm.IdCorral,
        IdAnimal = vm.IdAnimal,
        IdRegistroProduccionLeche = vm.IdRegistroProduccionLeche,
        FechaRegistro = DateTime.Now
      };

      _context.MovimientoCostos.Add(entity);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var entity = await _context.MovimientoCostos.FindAsync(id);
      if (entity == null) return NotFound();

      var vm = new MovimientoCostoVm
      {
        IdMovimientoCosto = entity.IdMovimientoCosto,
        Fecha = entity.Fecha.ToDateTime(TimeOnly.MinValue),
        IdCentroCosto = entity.IdCentroCosto,
        IdTipoCosto = entity.IdTipoCosto,
        MontoTotal = entity.MontoTotal,
        Descripcion = entity.Descripcion,
        IdEstablo = entity.IdEstablo,
        IdCorral = entity.IdCorral,
        IdAnimal = entity.IdAnimal,
        IdRegistroProduccionLeche = entity.IdRegistroProduccionLeche
      };
      ViewBag.Producciones = _context.RegistroProduccionLeches
      .Include(r => r.idAnimalNavigation)
      .Include(r => r.Calidads)
      .AsQueryable();

      await CargarCombos(vm);
      return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MovimientoCostoVm vm)
    {
      if (!ModelState.IsValid)
      {
        await CargarCombos(vm);
        return View(vm);
      }

      var entity = await _context.MovimientoCostos.FindAsync(id);
      if (entity == null) return NotFound();

      if (vm.IdAnimal.HasValue)
      {
        var animal = await _context.Animals
            .Include(a => a.idHatoNavigation)
            .ThenInclude(h => h.Establo)
            .FirstOrDefaultAsync(a => a.Id == vm.IdAnimal.Value);

        if (animal != null)
        {
          vm.IdCorral ??= animal.idHato;
          vm.IdEstablo ??= animal.idHatoNavigation.EstabloId;
        }
      }

      entity.Fecha = DateOnly.FromDateTime(vm.Fecha.Date);
      entity.IdCentroCosto = vm.IdCentroCosto;
      entity.IdTipoCosto = vm.IdTipoCosto;
      entity.MontoTotal = vm.MontoTotal;
      entity.Descripcion = vm.Descripcion;
      entity.IdEstablo = vm.IdEstablo;
      entity.IdCorral = vm.IdCorral;
      entity.IdAnimal = vm.IdAnimal;
      entity.IdRegistroProduccionLeche = vm.IdRegistroProduccionLeche;

      _context.Update(entity);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
      var entity = await _context.MovimientoCostos
          .Include(x => x.IdCentroCostoNavigation)
          .Include(x => x.IdTipoCostoNavigation)
          .FirstOrDefaultAsync(x => x.IdMovimientoCosto == id);

      if (entity == null) return NotFound();
      return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var entity = await _context.MovimientoCostos.FindAsync(id);
      if (entity == null) return NotFound();

      _context.MovimientoCostos.Remove(entity);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    private async Task CargarCombos(MovimientoCostoVm vm)
    {
      vm.CentrosCosto = await _context.CentroCostos
          .Where(c => c.Activo)
          .OrderBy(c => c.Nombre)
          .Select(c => new SelectListItem { Value = c.IdCentroCosto.ToString(), Text = c.Nombre })
          .ToListAsync();

      vm.TiposCosto = await _context.TipoCostos
          .OrderBy(t => t.Nombre)
          .Select(t => new SelectListItem { Value = t.IdTipoCosto.ToString(), Text = t.Nombre })
          .ToListAsync();

      vm.Establos = await _context.Establos
          .OrderBy(e => e.nombre)
          .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.nombre })
          .ToListAsync();

      vm.Hatos = await _context.Hatos
          .OrderBy(h => h.nombre)
          .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.nombre })
          .ToListAsync();

      vm.Animales = await _context.Animals
          .OrderBy(a => a.codigo)
          .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.codigo} - {a.nombre}" })
          .ToListAsync();
    }
  }
}
