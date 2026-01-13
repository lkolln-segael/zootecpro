
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Inseminador;

namespace WebZootecPro.Controllers;

public class InseminadoresController : Controller
{
  private readonly ZootecContext _context;

  public InseminadoresController(ZootecContext context)
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
    var establo = await _context.Establos.FirstOrDefaultAsync(e => empresa != null && e.EmpresaId == empresa.Id);
    return establo != null ? establo.Id : -1;
  }
  private async Task<Empresa?> GetEmpresaAsync()
  {
    var usuario = await GetUsuarioActualAsync();
    if (usuario == null)
      return null;

    return await _context.Empresas
        .FirstOrDefaultAsync(e => e.usuarioID == usuario.Id
            || (e.Colaboradors != null
                && e.Colaboradors.Select(c => c.idUsuario).Contains(usuario.Id)));
  }

  // GET: Inseminadors
  public async Task<IActionResult> Index()
  {
    var list = await _context.Inseminadors
        .AsNoTracking()
        .Select(i => new InseminadorViewModel
        {
          Id = i.Id,
          Nombre = i.nombre,
          Apellido = i.apellido
        })
        .ToListAsync();

    return View(list);
  }

  // GET: Inseminadors/Details/5
  public async Task<IActionResult> Details(int id)
  {
    var i = await _context.Inseminadors
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new InseminadorViewModel
        {
          Id = x.Id,
          Nombre = x.nombre,
          Apellido = x.apellido
        })
        .FirstOrDefaultAsync();

    if (i == null) return NotFound();

    return View(i);
  }

  // GET: Inseminadors/Create
  public IActionResult Create()
  {
    return View(new InseminadorViewModel());
  }

  // POST: Inseminadors/Create
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(InseminadorViewModel vm)
  {
    var empresa = await GetEmpresaAsync();
    if (!ModelState.IsValid) return View(vm);

    var entity = new Inseminador
    {
      nombre = vm.Nombre,
      apellido = vm.Apellido,
      EmpresaId = empresa.Id
    };

    _context.Add(entity);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  // GET: Inseminadors/Edit/5
  public async Task<IActionResult> Edit(int id)
  {
    var i = await _context.Inseminadors.FindAsync(id);
    if (i == null) return NotFound();

    var vm = new InseminadorViewModel
    {
      Id = i.Id,
      Nombre = i.nombre,
      Apellido = i.apellido
    };

    return View(vm);
  }

  // POST: Inseminadors/Edit/5
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, InseminadorViewModel vm)
  {
    if (id != vm.Id) return BadRequest();
    if (!ModelState.IsValid) return View(vm);

    var i = await _context.Inseminadors.FindAsync(id);
    if (i == null) return NotFound();

    i.nombre = vm.Nombre;
    i.apellido = vm.Apellido;

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
  }

  // GET: Inseminadors/Delete/5
  public async Task<IActionResult> Delete(int id)
  {
    var i = await _context.Inseminadors
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new InseminadorViewModel
        {
          Id = x.Id,
          Nombre = x.nombre,
          Apellido = x.apellido
        })
        .FirstOrDefaultAsync();

    if (i == null) return NotFound();

    return View(i);
  }

  // POST: Inseminadors/Delete/5
  [HttpPost, ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    var i = await _context.Inseminadors.FindAsync(id);
    if (i == null) return NotFound();

    _context.Inseminadors.Remove(i);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }
}
