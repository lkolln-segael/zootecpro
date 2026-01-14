using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Inseminador;

namespace WebZootecPro.Controllers;

[Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,VETERINARIO,USUARIO_EMPRESA")]
public class InseminadoresController : Controller
{
    private readonly ZootecContext _context;

    public InseminadoresController(ZootecContext context)
    {
        _context = context;
    }

    private bool IsSuperAdmin => User.IsInRole("SUPERADMIN");

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

    private async Task<Empresa?> GetEmpresaAsync()
    {
        var usuario = await GetUsuarioActualAsync();
        if (usuario == null) return null;

        return await _context.Empresas.FirstOrDefaultAsync(e =>
          e.usuarioID == usuario.Id
          || (e.Colaboradors != null && e.Colaboradors.Select(c => c.idUsuario).Contains(usuario.Id)));
    }

    private async Task<int?> GetEmpresaIdScopeAsync()
    {
        if (IsSuperAdmin) return null; // SUPERADMIN ve todo (si quieres que NO, quita esto y siempre retorna empresa.Id)
        var empresa = await GetEmpresaAsync();
        return empresa?.Id;
    }

    private static IQueryable<Inseminador> Scope(IQueryable<Inseminador> q, int? empresaId)
      => empresaId == null ? q : q.Where(x => x.EmpresaId == empresaId.Value);

    // GET: Inseminadores
    public async Task<IActionResult> Index()
    {
        var empresaId = await GetEmpresaIdScopeAsync();
        if (!IsSuperAdmin && empresaId == null) return Forbid();

        var list = await Scope(_context.Inseminadors.AsNoTracking(), empresaId)
          .OrderBy(x => x.apellido).ThenBy(x => x.nombre)
          .Select(i => new InseminadorViewModel
          {
              Id = i.Id,
              Nombre = i.nombre,
              Apellido = i.apellido
          })
          .ToListAsync();

        return View(list);
    }

    // GET: Inseminadores/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var empresaId = await GetEmpresaIdScopeAsync();
        if (!IsSuperAdmin && empresaId == null) return Forbid();

        var i = await Scope(_context.Inseminadors.AsNoTracking(), empresaId)
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

    // GET: Inseminadores/Create
    public IActionResult Create() => View(new InseminadorViewModel());

    // POST: Inseminadores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InseminadorViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var empresa = await GetEmpresaAsync();
        if (empresa == null) return Forbid(); // obligatorio para asignar EmpresaId

        var entity = new Inseminador
        {
            nombre = vm.Nombre,
            apellido = vm.Apellido,
            EmpresaId = empresa.Id
        };

        _context.Inseminadors.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Inseminadores/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var empresaId = await GetEmpresaIdScopeAsync();
        if (!IsSuperAdmin && empresaId == null) return Forbid();

        var i = await Scope(_context.Inseminadors.AsNoTracking(), empresaId)
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

    // POST: Inseminadores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InseminadorViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var empresaId = await GetEmpresaIdScopeAsync();
        if (!IsSuperAdmin && empresaId == null) return Forbid();

        // NO usar FindAsync(id) sin validar EmpresaId
        var i = await Scope(_context.Inseminadors, empresaId)
          .FirstOrDefaultAsync(x => x.Id == id);

        if (i == null) return NotFound();

        i.nombre = vm.Nombre;
        i.apellido = vm.Apellido;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Inseminadores/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var empresaId = await GetEmpresaIdScopeAsync();
        if (!IsSuperAdmin && empresaId == null) return Forbid();

        var i = await Scope(_context.Inseminadors.AsNoTracking(), empresaId)
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

    // POST: Inseminadores/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var empresaId = await GetEmpresaIdScopeAsync();
        if (!IsSuperAdmin && empresaId == null) return Forbid();

        var i = await Scope(_context.Inseminadors, empresaId)
          .FirstOrDefaultAsync(x => x.Id == id);

        if (i == null) return NotFound();

        _context.Inseminadors.Remove(i);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
