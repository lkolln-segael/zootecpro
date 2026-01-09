using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
  public class HatosController : Controller
  {
    private readonly ZootecContext _context;

    public HatosController(ZootecContext context)
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


    // GET: Hatos
    public async Task<IActionResult> Index()
    {
      var establo = await GetEstabloScopeAsync();
      var hatos = await _context.Hatos.Where(h => h.EstabloId == establo).ToListAsync();

      ViewBag.Establos = await _context.Establos
          .Where(e => e.Id == establo)
          .ToDictionaryAsync(e => e.Id, e => e.nombre);

      return View(hatos);
    }

    // GET: Hatos/Create
    // GET: Hatos/Create
    public async Task<IActionResult> Create()
    {
      var empresa = await GetEmpresaAsync();
      ViewBag.IdEstablo = new SelectList(
          await _context.Establos.Where(e => e.EmpresaId == empresa.Id).OrderBy(e => e.nombre).ToListAsync(),
          "Id",          // ✅ mayúscula
          "nombre"
      );

      return View(new Hato());
    }

    // POST: Hatos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Hato hato)
    {
      ModelState.Remove(nameof(Hato.Establo));   // navegación principal
      ModelState.Remove(nameof(Hato.Usuarios));  // colecciones (si las tienes)
      ModelState.Remove(nameof(Hato.Animals));
      // si tienes cosas extra aquí, déjalas
      if (!ModelState.IsValid)
      {
        ViewBag.IdEstablo = new SelectList(
            await _context.Establos.OrderBy(e => e.nombre).ToListAsync(),
            "Id",          // ✅
            "nombre",
            hato.EstabloId
        );
        return View(hato);
      }

      _context.Hatos.Add(hato);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    // GET: Hatos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null) return NotFound();

      var hato = await _context.Hatos.FindAsync(id);
      if (hato == null) return NotFound();

      ViewBag.IdEstablo = new SelectList(
          await _context.Establos.OrderBy(e => e.nombre).ToListAsync(),
          "Id",           // ✅
          "nombre",
          hato.EstabloId
      );

      return View(hato);
    }

    // POST: Hatos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Hato hato)
    {
      if (id != hato.Id) return NotFound();

      if (!ModelState.IsValid)
      {
        ViewBag.IdEstablo = new SelectList(
            await _context.Establos.OrderBy(e => e.nombre).ToListAsync(),
            "Id",          // ✅
            "nombre",
            hato.EstabloId
        );
        return View(hato);
      }

      _context.Update(hato);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    // GET: Hatos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null) return NotFound();

      var hato = await _context.Hatos.FirstOrDefaultAsync(h => h.Id == id);
      if (hato == null) return NotFound();

      return View(hato);
    }

    // POST: Hatos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var hato = await _context.Hatos.FindAsync(id);
      if (hato != null)
      {
        _context.Hatos.Remove(hato);
        await _context.SaveChangesAsync();
      }
      return RedirectToAction(nameof(Index));
    }
  }
}
