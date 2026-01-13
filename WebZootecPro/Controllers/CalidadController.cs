using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,LABORATORIO_EMPRESA")]
  public class CalidadController : Controller
  {
    private readonly ZootecContext _context;

    public CalidadController(ZootecContext context)
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
      return await _context.Empresas.FirstOrDefaultAsync(e => e.usuarioID == usuario.Id
          || e.Colaboradors.Select(e => e.idUsuario).Contains(usuario.Id));
    }
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string? turno, int? idAnimal)
    {
      var establoId = await GetEstabloScopeAsync();
      var q = _context.RegistroProduccionLeches
          .Where(r => r.idAnimalNavigation.idHatoNavigation.EstabloId == establoId)
          .Include(r => r.idAnimalNavigation)
          .Include(r => r.Calidads)
          .AsQueryable();

      if (desde.HasValue)
        q = q.Where(r => r.fechaOrdeno >= desde.Value.Date);

      if (hasta.HasValue)
        q = q.Where(r => r.fechaOrdeno <= hasta.Value.Date.AddDays(1).AddTicks(-1));

      if (!string.IsNullOrWhiteSpace(turno)) q = q.Where(r => r.turno == turno);
      if (idAnimal.HasValue) q = q.Where(r => r.idAnimal == idAnimal.Value);

      var lista = await q
          .OrderByDescending(r => r.fechaOrdeno)
          .ThenBy(r => r.turno)
          .ToListAsync();

      ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
      ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

      ViewBag.IdAnimal = new SelectList(
          await _context.Animals.OrderBy(a => a.codigo).ToListAsync(),
          "Id", "codigo", idAnimal);

      return View(lista);
    }


    [HttpGet]
    public async Task<IActionResult> Editar(int idRegistro)
    {
      var reg = await _context.RegistroProduccionLeches
          .Include(r => r.idAnimalNavigation)
          .Include(r => r.Calidads)
          .FirstOrDefaultAsync(r => r.Id == idRegistro);

      if (reg == null) return NotFound();

      var calidad = reg.Calidads
          .OrderByDescending(c => c.fechaRegistro)
          .FirstOrDefault();

      ViewBag.Animal = reg.idAnimalNavigation?.codigo;
      ViewBag.Fecha = reg.fechaOrdeno?.ToString("dd/MM/yyyy");
      ViewBag.Turno = reg.turno;
      ViewBag.IdRegistro = reg.Id;

      return View(calidad ?? new Calidad { idRegistroProduccionLeche = reg.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int idRegistro, Calidad model)
    {
      if (idRegistro != model.idRegistroProduccionLeche)
        return BadRequest();

      // valida que existe el registro
      var existeReg = await _context.RegistroProduccionLeches.AnyAsync(r => r.Id == idRegistro);
      if (!existeReg) return NotFound();

      Calidad? actual = await _context.Calidads
          .OrderByDescending(c => c.fechaRegistro)
          .FirstOrDefaultAsync(c => c.idRegistroProduccionLeche == idRegistro);

      if (actual == null)
      {
        model.fechaRegistro = DateTime.Now;
        _context.Calidads.Add(model);
      }
      else
      {
        // actualización (si prefieres historial, en vez de actualizar crea un nuevo registro)
        actual.grasa = model.grasa;
        actual.proteina = model.proteina;
        actual.solidosTotales = model.solidosTotales;
        actual.urea = model.urea;
        actual.rcs = model.rcs;
        actual.fechaRegistro = DateTime.Now;
      }

      await _context.SaveChangesAsync();
      return RedirectToAction("Index", "Calidad");
    }
  }
}
