using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.AlimentacionRtm;

namespace WebZootecPro.Controllers;

[Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
public class AlimentacionRtmController : Controller
{
  private readonly ZootecContext _context;

  public AlimentacionRtmController(ZootecContext context)
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
    if (usuario == null) return null;

    return await _context.Empresas.FirstOrDefaultAsync(e =>
        e.usuarioID == usuario.Id
        || e.Colaboradors.Any(c => c.idUsuario == usuario.Id));
  }
  private async Task CargarHatosAsync(Usuario? u, int? hatoIdSel)
  {
    var establo = await GetEstabloScopeAsync();
    var q = _context.Hatos.Where(e => e.EstabloId == establo).AsNoTracking().AsQueryable();

    if (u?.idEstablo != null)
      q = q.Where(h => h.EstabloId == u.idEstablo.Value);

    if (u?.idHato != null)
      q = q.Where(h => h.Id == u.idHato.Value);

    var hatos = await q.OrderBy(h => h.nombre).ToListAsync();
    ViewBag.HatoId = new SelectList(hatos, "Id", "nombre", hatoIdSel);
  }

  private async Task CargarFormulasAsync(int? formulaSel = null)
  {
    var formulas = await _context.RtmFormulas.AsNoTracking()
        .OrderBy(f => f.nombre).ToListAsync();
    ViewBag.FormulaId = new SelectList(formulas, "Id", "nombre", formulaSel);
  }

  private async Task CargarIngredientesAsync(int? ingredienteSel = null)
  {
    var ing = await _context.RtmIngredientes.AsNoTracking()
        .Where(i => i.activo)
        .OrderBy(i => i.nombre).ToListAsync();
    ViewBag.IngredienteId = new SelectList(ing, "Id", "nombre", ingredienteSel);
  }

  private async Task<int> ContarVacasActivasEnHatoAsync(int hatoId)
  {
    // Ajusta si quieres solo HEMBRA o estado != INACTIVO
    return await _context.Animals.AsNoTracking()
        .Where(a => a.idHato == hatoId)
        .Where(a => a.estado == null || a.estado.nombre != "INACTIVO")
        .CountAsync();
  }

  // =========================
  // A) INGREDIENTES
  // =========================
  public async Task<IActionResult> Ingredientes()
  {
    var items = await _context.RtmIngredientes.AsNoTracking()
        .OrderBy(i => i.nombre)
        .Select(i => new IngredienteVm
        {
          Id = i.Id,
          nombre = i.nombre,
          unidad = i.unidad,
          costoKg = i.costoKg,
          msPct = i.msPct,
          activo = i.activo
        }).ToListAsync();

    return View(items);
  }

  [HttpGet]
  public IActionResult IngredienteCrear() => View(new IngredienteVm { activo = true });

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> IngredienteCrear(IngredienteVm vm)
  {
    if (!ModelState.IsValid) return View(vm);

    var e = new RtmIngrediente
    {
      nombre = vm.nombre.Trim(),
      unidad = vm.unidad,
      costoKg = vm.costoKg,
      msPct = vm.msPct,
      activo = vm.activo
    };

    _context.RtmIngredientes.Add(e);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Ingredientes));
  }

  [HttpGet]
  public async Task<IActionResult> IngredienteEditar(int id)
  {
    var e = await _context.RtmIngredientes.FindAsync(id);
    if (e == null) return NotFound();

    return View(new IngredienteVm
    {
      Id = e.Id,
      nombre = e.nombre,
      unidad = e.unidad,
      costoKg = e.costoKg,
      msPct = e.msPct,
      activo = e.activo
    });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> IngredienteEditar(IngredienteVm vm)
  {
    var e = await _context.RtmIngredientes.FindAsync(vm.Id);
    if (e == null) return NotFound();

    if (!ModelState.IsValid) return View(vm);

    e.nombre = vm.nombre.Trim();
    e.unidad = vm.unidad;
    e.costoKg = vm.costoKg;
    e.msPct = vm.msPct;
    e.activo = vm.activo;

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Ingredientes));
  }

  // =========================
  // B) FORMULAS RTM
  // =========================
  public async Task<IActionResult> Formulas()
  {
    var items = await _context.RtmFormulas.AsNoTracking()
        .OrderBy(f => f.nombre)
        .Select(f => new { f.Id, f.nombre, f.activo, f.costoKg })
        .ToListAsync();

    return View(items);
  }

  [HttpGet]
  public IActionResult FormulaCrear()
  {
    return View(new RtmFormula { activo = true, fechaCreacion = DateTime.Now });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> FormulaCrear(RtmFormula f)
  {
    if (string.IsNullOrWhiteSpace(f.nombre))
      ModelState.AddModelError(nameof(f.nombre), "Nombre obligatorio");

    if (!ModelState.IsValid) return View(f);

    f.nombre = f.nombre.Trim();
    f.fechaCreacion = DateTime.Now;

    _context.RtmFormulas.Add(f);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(FormulaEditar), new { id = f.Id });
  }

  [HttpGet]
  public async Task<IActionResult> FormulaEditar(int id)
  {
    var f = await _context.RtmFormulas
        .Include(x => x.RtmFormulaDetalles).ThenInclude(d => d.ingrediente)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (f == null) return NotFound();

    await CargarIngredientesAsync();

    var vm = new FormulaEditVm
    {
      Id = f.Id,
      nombre = f.nombre,
      descripcion = f.descripcion,
      activo = f.activo,
      costoKg = f.costoKg,
      detalles = f.RtmFormulaDetalles.OrderBy(d => d.Id).Select(d => new FormulaDetalleRowVm
      {
        Id = d.Id,
        ingredienteId = d.ingredienteId,
        ingrediente = d.ingrediente.nombre,
        porcentaje = d.porcentaje,
        costoKg = d.ingrediente.costoKg
      }).ToList()
    };

    vm.porcentajeTotal = vm.detalles.Sum(x => x.porcentaje);

    return View(vm);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> FormulaActualizarCabecera(FormulaEditVm vm)
  {
    var f = await _context.RtmFormulas.FindAsync(vm.Id);
    if (f == null) return NotFound();

    if (string.IsNullOrWhiteSpace(vm.nombre))
      ModelState.AddModelError(nameof(vm.nombre), "Nombre obligatorio");

    if (!ModelState.IsValid)
    {
      await CargarIngredientesAsync(vm.ingredienteId);
      return View("FormulaEditar", await RebuildFormulaEditVm(vm.Id));
    }

    f.nombre = vm.nombre.Trim();
    f.descripcion = vm.descripcion;
    f.activo = vm.activo;

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(FormulaEditar), new { id = vm.Id });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> FormulaAgregarDetalle(FormulaEditVm vm)
  {
    if (vm.ingredienteId <= 0)
      ModelState.AddModelError(nameof(vm.ingredienteId), "Selecciona ingrediente");

    if (vm.porcentaje <= 0)
      ModelState.AddModelError(nameof(vm.porcentaje), "Porcentaje debe ser > 0");

    if (!ModelState.IsValid)
    {
      await CargarIngredientesAsync(vm.ingredienteId);
      return View("FormulaEditar", await RebuildFormulaEditVm(vm.Id));
    }

    // evitar duplicado del mismo ingrediente
    var existe = await _context.RtmFormulaDetalles
        .AnyAsync(d => d.formulaId == vm.Id && d.ingredienteId == vm.ingredienteId);

    if (existe)
    {
      TempData["msg"] = "Ese ingrediente ya está en la fórmula.";
      return RedirectToAction(nameof(FormulaEditar), new { id = vm.Id });
    }

    _context.RtmFormulaDetalles.Add(new RtmFormulaDetalle
    {
      formulaId = vm.Id,
      ingredienteId = vm.ingredienteId,
      porcentaje = vm.porcentaje,
      observacion = vm.observacion
    });

    await _context.SaveChangesAsync();

    await RecalcularCostoFormulaAsync(vm.Id);

    return RedirectToAction(nameof(FormulaEditar), new { id = vm.Id });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> FormulaEliminarDetalle(int id, int formulaId)
  {
    var d = await _context.RtmFormulaDetalles.FindAsync(id);
    if (d == null) return NotFound();

    _context.RtmFormulaDetalles.Remove(d);
    await _context.SaveChangesAsync();

    await RecalcularCostoFormulaAsync(formulaId);

    return RedirectToAction(nameof(FormulaEditar), new { id = formulaId });
  }

  private async Task<FormulaEditVm> RebuildFormulaEditVm(int formulaId)
  {
    var f = await _context.RtmFormulas
        .Include(x => x.RtmFormulaDetalles).ThenInclude(d => d.ingrediente)
        .FirstAsync(x => x.Id == formulaId);

    return new FormulaEditVm
    {
      Id = f.Id,
      nombre = f.nombre,
      descripcion = f.descripcion,
      activo = f.activo,
      costoKg = f.costoKg,
      detalles = f.RtmFormulaDetalles.Select(d => new FormulaDetalleRowVm
      {
        Id = d.Id,
        ingredienteId = d.ingredienteId,
        ingrediente = d.ingrediente.nombre,
        porcentaje = d.porcentaje,
        costoKg = d.ingrediente.costoKg
      }).ToList(),
      porcentajeTotal = f.RtmFormulaDetalles.Sum(x => x.porcentaje)
    };
  }

  private async Task RecalcularCostoFormulaAsync(int formulaId)
  {
    var detalles = await _context.RtmFormulaDetalles
        .Include(d => d.ingrediente)
        .Where(d => d.formulaId == formulaId)
        .ToListAsync();

    // costoKg = sum(pct/100 * costoKgIngrediente)
    decimal? costo = 0m;
    foreach (var d in detalles)
    {
      if (d.ingrediente.costoKg == null) continue;
      costo += (d.porcentaje / 100m) * d.ingrediente.costoKg.Value;
    }

    var f = await _context.RtmFormulas.FindAsync(formulaId);
    if (f != null)
    {
      f.costoKg = detalles.Count == 0 ? null : Math.Round(costo ?? 0m, 4);
      await _context.SaveChangesAsync();
    }
  }

  // =========================
  // C) PROGRAMACION POR CORRAL (HATO)
  // =========================
  [HttpGet]
  public async Task<IActionResult> Programacion(int? hatoId)
  {
    var u = await GetUsuarioActualAsync();
    await CargarHatosAsync(u, hatoId);
    await CargarFormulasAsync();

    var q = _context.RtmRacionCorrals.AsNoTracking()
        .Include(r => r.hato)
        .Include(r => r.formula)
        .AsQueryable();

    if (hatoId != null) q = q.Where(r => r.hatoId == hatoId.Value);

    var items = await q.OrderBy(r => r.hato.nombre).ThenBy(r => r.hora)
        .Select(r => new ProgramacionRowVm
        {
          Id = r.Id,
          hato = r.hato.nombre,
          formula = r.formula.nombre,
          hora = r.hora,
          kgRtmPorVaca = r.kgRtmPorVaca,
          activo = r.activo
        }).ToListAsync();

    return View(new ProgramacionVm { hatoId = hatoId, items = items });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ProgramacionCrear(ProgramacionVm vm)
  {
    var u = await GetUsuarioActualAsync();
    await CargarHatosAsync(u, vm.hatoId);
    await CargarFormulasAsync(vm.formulaId);

    if (vm.hatoId == null || vm.hatoId <= 0)
      ModelState.AddModelError(nameof(vm.hatoId), "Selecciona Hato/Corral");

    if (vm.formulaId <= 0)
      ModelState.AddModelError(nameof(vm.formulaId), "Selecciona fórmula");

    if (vm.kgRtmPorVaca <= 0)
      ModelState.AddModelError(nameof(vm.kgRtmPorVaca), "kg/vaca debe ser > 0");

    if (!ModelState.IsValid)
      return View("Programacion", await BuildProgramacionVm(vm.hatoId));

    _context.RtmRacionCorrals.Add(new RtmRacionCorral
    {
      hatoId = vm.hatoId.Value,
      formulaId = vm.formulaId,
      hora = vm.hora,
      kgRtmPorVaca = vm.kgRtmPorVaca,
      activo = true,
      observacion = vm.observacion
    });

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Programacion), new { hatoId = vm.hatoId });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ProgramacionToggle(int id, int? hatoId)
  {
    var r = await _context.RtmRacionCorrals.FindAsync(id);
    if (r == null) return NotFound();

    r.activo = !r.activo;
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Programacion), new { hatoId });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ProgramacionEliminar(int id, int? hatoId)
  {
    var r = await _context.RtmRacionCorrals.FindAsync(id);
    if (r == null) return NotFound();

    _context.RtmRacionCorrals.Remove(r);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Programacion), new { hatoId });
  }

  private async Task<ProgramacionVm> BuildProgramacionVm(int? hatoId)
  {
    var q = _context.RtmRacionCorrals.AsNoTracking()
        .Include(r => r.hato)
        .Include(r => r.formula)
        .AsQueryable();

    if (hatoId != null) q = q.Where(r => r.hatoId == hatoId.Value);

    var items = await q.OrderBy(r => r.hora).Select(r => new ProgramacionRowVm
    {
      Id = r.Id,
      hato = r.hato.nombre,
      formula = r.formula.nombre,
      hora = r.hora,
      kgRtmPorVaca = r.kgRtmPorVaca,
      activo = r.activo
    }).ToListAsync();

    return new ProgramacionVm { hatoId = hatoId, items = items };
  }

  // =========================
  // D) ENTREGAS / CONSUMO REAL
  // =========================
  [HttpGet]
  public async Task<IActionResult> Entregas(int? hatoId, DateOnly? fecha)
  {
    var u = await GetUsuarioActualAsync();
    var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);

    await CargarHatosAsync(u, hatoId);
    await CargarFormulasAsync();

    var q = _context.RtmEntregas.AsNoTracking()
        .Include(e => e.hato)
        .Include(e => e.formula)
        .AsQueryable();

    q = q.Where(e => e.fecha == f);
    if (hatoId != null) q = q.Where(e => e.hatoId == hatoId.Value);

    var list = await q.OrderBy(e => e.hora).ToListAsync();

    var items = list.Select(e =>
    {
      var costoKg = e.formula.costoKg;
      return new EntregaRowVm
      {
        Id = e.Id,
        hato = e.hato.nombre,
        formula = e.formula.nombre,
        fecha = e.fecha,
        hora = e.hora,
        kgTotal = e.kgTotal,
        numeroVacas = e.numeroVacas,
        kgPorVaca = e.kgPorVaca,
        costoKgFormula = costoKg,
        costoTotal = costoKg == null ? null : Math.Round(costoKg.Value * e.kgTotal, 2)
      };
    }).ToList();

    var vm = new EntregasVm
    {
      hatoId = hatoId,
      fecha = f,
      items = items,
      hora = new TimeOnly(6, 0),
      numeroVacas = (hatoId != null) ? await ContarVacasActivasEnHatoAsync(hatoId.Value) : 0
    };

    return View(vm);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> EntregasCrear(EntregasVm vm)
  {
    var u = await GetUsuarioActualAsync();
    await CargarHatosAsync(u, vm.hatoId);
    await CargarFormulasAsync(vm.formulaId);

    if (vm.hatoId == null || vm.hatoId <= 0)
      ModelState.AddModelError(nameof(vm.hatoId), "Selecciona Hato/Corral");

    if (vm.formulaId <= 0)
      ModelState.AddModelError(nameof(vm.formulaId), "Selecciona fórmula");

    if (vm.kgTotal <= 0)
      ModelState.AddModelError(nameof(vm.kgTotal), "kg total debe ser > 0");

    if (vm.numeroVacas <= 0)
      ModelState.AddModelError(nameof(vm.numeroVacas), "número vacas debe ser > 0");

    if (!ModelState.IsValid)
      return View("Entregas", vm);

    var kgPorVaca = Math.Round(vm.kgTotal / vm.numeroVacas, 4);

    _context.RtmEntregas.Add(new RtmEntrega
    {
      hatoId = vm.hatoId.Value,
      formulaId = vm.formulaId,
      fecha = vm.fecha,
      hora = vm.hora,
      kgTotal = vm.kgTotal,
      numeroVacas = vm.numeroVacas,
      kgPorVaca = kgPorVaca,
      idUsuario = GetUsuarioId(),
      observacion = vm.observacion
    });

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Entregas), new { hatoId = vm.hatoId, fecha = vm.fecha });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> EntregaEliminar(int id, int? hatoId, DateOnly fecha)
  {
    var e = await _context.RtmEntregas.FindAsync(id);
    if (e == null) return NotFound();

    _context.RtmEntregas.Remove(e);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Entregas), new { hatoId, fecha });
  }

  // =========================
  // E) REPORTE CONSUMO (por fecha / corral)
  // =========================
  [HttpGet]
  public async Task<IActionResult> Consumo(int? hatoId, DateOnly? desde, DateOnly? hasta)
  {
    var u = await GetUsuarioActualAsync();

    var d = desde ?? DateOnly.FromDateTime(DateTime.Today).AddDays(-7);
    var h = hasta ?? DateOnly.FromDateTime(DateTime.Today);

    if (h < d) { var t = d; d = h; h = t; }

    await CargarHatosAsync(u, hatoId);

    var q = _context.RtmEntregas.AsNoTracking()
        .Include(e => e.hato)
        .Include(e => e.formula)
        .Where(e => e.fecha >= d && e.fecha <= h)
        .AsQueryable();

    if (hatoId != null) q = q.Where(e => e.hatoId == hatoId.Value);

    var rows = await q.ToListAsync();

    var items = rows
        .GroupBy(x => new { x.fecha, x.hato.nombre })
        .OrderBy(g => g.Key.fecha)
        .ThenBy(g => g.Key.nombre)
        .Select(g =>
        {
          var kg = g.Sum(x => x.kgTotal);
          decimal? costo = null;
          var anyCosto = g.Any(x => x.formula.costoKg != null);
          if (anyCosto)
          {
            costo = Math.Round(g.Sum(x => (x.formula.costoKg ?? 0m) * x.kgTotal), 2);
          }

          return new ConsumoRowVm
          {
            fecha = g.Key.fecha,
            hato = g.Key.nombre,
            kgTotal = kg,
            costoTotal = costo
          };
        })
        .ToList();

    return View(new ConsumoVm
    {
      hatoId = hatoId,
      desde = d,
      hasta = h,
      items = items
    });
  }
}
