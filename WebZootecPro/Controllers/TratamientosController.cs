using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Tratamientos;

namespace WebZootecPro.Controllers
{
  public class TratamientosController : Controller
  {
    private readonly ZootecContext _context;

    public TratamientosController(ZootecContext context)
    {
      _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var hoy = DateTime.Today;

      var retiroDict = await _context.TipoTratamientos
          .AsNoTracking()
          .ToDictionaryAsync(t => t.Id, t => t.retiroLecheDias);

      ViewBag.RetiroLecheDias = retiroDict;

      // Diccionarios que ya usas
      var enfermedadesDict = await _context.Enfermedads
          .AsNoTracking()
          .ToDictionaryAsync(e => e.Id, e => e.fechaDiagnostico);

      var tipoTratamientoDict = await _context.TipoTratamientos
          .AsNoTracking()
          .ToDictionaryAsync(tt => tt.Id, tt => tt.nombre);

      ViewBag.IdEnfermedad = enfermedadesDict;
      ViewBag.IdTipoTratamiento = tipoTratamientoDict;

      // Carga tratamientos con navegaciones necesarias
      var tratamientos = await _context.Tratamientos
          .AsNoTracking()
          .Include(t => t.idTipoTratamientoNavigation)
          .ToListAsync();

      var model = tratamientos.Select(t =>
      {
        var fechaInicio = t.fechaInicio.Date;

        // Última dosis (misma lógica que Producción)
        var ultimaDosis = (t.fechaFinalEstimada ?? t.fechaInicio).Date;

        // Días en enfermería: si no hay fechaFinal real, cuenta hasta hoy
        var finEnfermeria = (t.fechaFinalEstimada ?? hoy).Date;
        var diasEnEnfermeria = (finEnfermeria - fechaInicio).Days + 1;
        if (diasEnEnfermeria < 0) diasEnEnfermeria = 0;

        // Retiro
        int? retiroDias = t.idTipoTratamientoNavigation?.retiroLecheDias;
        DateTime? retiroHasta = null;

        if (retiroDias.HasValue && retiroDias.Value > 0)
        {
          // Inclusive al final del día
          retiroHasta = ultimaDosis
                    .AddDays(retiroDias.Value)
                    .AddDays(1)
                    .AddTicks(-1);
        }

        return new CrearTratamientoViewModel
        {
          IdTratamiento = t.Id,
          FechaInicio = t.fechaInicio,
          FechaFinalEstimada = t.fechaFinalEstimada,  // ahora nullable
          CostoEstimado = t.costoEstimado ?? 0,
          Observaciones = t.observaciones ?? "No hay observaciones",
          IdTipoTratamiento = t.idTipoTratamiento,
          IdEnfermedad = t.idEnfermedad,

          DiasEnEnfermeria = diasEnEnfermeria,
          UltimaDosis = ultimaDosis,
          RetiroHasta = retiroHasta,
          EnRetiroHoy = retiroHasta != null && DateTime.Now <= retiroHasta.Value
        };
      }).ToList();

      return View(model);
    }


    [HttpGet]
    public IActionResult Create()
    {
      var model = new CrearTratamientoViewModel
      {
        TipoTratamientos = _context.TipoTratamientos
              .Select(t => new SelectListItem
              {
                Value = t.Id.ToString(),
                Text = t.nombre
              })
              .ToList(),

        Enfermedades = _context.Enfermedads
              .Select(e => new SelectListItem
              {
                Value = e.Id.ToString(),
                Text = $"Enfermedad #{e.Id}"
              })
              .ToList(),
        FechaInicio = DateTime.Now
      };

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearTratamientoViewModel model)
    {
      if (model.FechaFinalEstimada != null && model.FechaFinalEstimada < model.FechaInicio)
      {
        ModelState.AddModelError("fechaFinalEstimada", "La fecha final tiene que ser mayor a la fecha de inicio");
      }
      if (!ModelState.IsValid)
      {
        model.TipoTratamientos = _context.TipoTratamientos
            .Select(t => new SelectListItem
            {
              Value = t.Id.ToString(),
              Text = t.nombre
            })
            .ToList();

        model.Enfermedades = _context.Enfermedads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = $"Enfermedad #{e.Id}"
            })
            .ToList();

        return View(model);
      }

      var tratamiento = new Tratamiento
      {
        fechaInicio = model.FechaInicio,
        fechaFinalEstimada = model.FechaFinalEstimada,
        costoEstimado = model.CostoEstimado,
        observaciones = model.Observaciones,
        idTipoTratamiento = model.IdTipoTratamiento,
        idEnfermedad = model.IdEnfermedad
      };

      _context.Tratamientos.Add(tratamiento);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var tratamiento = await _context.Tratamientos.FindAsync(id);
      if (tratamiento == null)
        return NotFound();

      var model = new CrearTratamientoViewModel
      {
        FechaInicio = tratamiento.fechaInicio,
        FechaFinalEstimada = tratamiento.fechaFinalEstimada ?? new DateTime(),
        CostoEstimado = tratamiento.costoEstimado ?? 0,
        Observaciones = tratamiento.observaciones ?? "No hubo observaciones",
        IdTipoTratamiento = tratamiento.idTipoTratamiento,
        IdEnfermedad = tratamiento.idEnfermedad,

        TipoTratamientos = _context.TipoTratamientos
              .Select(t => new SelectListItem
              {
                Value = t.Id.ToString(),
                Text = t.nombre
              })
              .ToList(),

        Enfermedades = _context.Enfermedads
              .Select(e => new SelectListItem
              {
                Value = e.Id.ToString(),
                Text = $"Enfermedad #{e.Id}"
              })
              .ToList()
      };

      ViewBag.Id = id;
      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CrearTratamientoViewModel model)
    {
      if (!ModelState.IsValid)
      {
        model.TipoTratamientos = _context.TipoTratamientos
            .Select(t => new SelectListItem
            {
              Value = t.Id.ToString(),
              Text = t.nombre
            })
            .ToList();

        model.Enfermedades = _context.Enfermedads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = $"Enfermedad #{e.Id}"
            })
            .ToList();

        ViewBag.Id = id;
        return View(model);
      }

      var tratamiento = await _context.Tratamientos.FindAsync(id);
      if (tratamiento == null)
        return NotFound();

      tratamiento.fechaInicio = model.FechaInicio;
      tratamiento.fechaFinalEstimada = model.FechaFinalEstimada;
      tratamiento.costoEstimado = model.CostoEstimado;
      tratamiento.observaciones = model.Observaciones;
      tratamiento.idTipoTratamiento = model.IdTipoTratamiento;
      tratamiento.idEnfermedad = model.IdEnfermedad;

      _context.Update(tratamiento);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
      var tratamiento = await _context.Tratamientos.FindAsync(id);
      if (tratamiento == null)
        return NotFound();

      var model = new CrearTratamientoViewModel
      {
        FechaInicio = tratamiento.fechaInicio,
        FechaFinalEstimada = tratamiento.fechaFinalEstimada ?? new DateTime(),
        CostoEstimado = tratamiento.costoEstimado ?? 0,
        Observaciones = tratamiento.observaciones ?? "No hubo observaciones",
        IdTipoTratamiento = tratamiento.idTipoTratamiento,
        IdEnfermedad = tratamiento.idEnfermedad
      };

      ViewBag.Id = id;
      return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> RetiroLeche(int dias = 7)
    {
      if (dias < 0) dias = 0;
      if (dias > 90) dias = 90; // límite sano

      var hoy = DateTime.Today;
      var hastaRango = hoy.AddDays(dias).AddDays(1).AddTicks(-1);

      var data = await _context.Tratamientos
          .AsNoTracking()
          .Include(t => t.idTipoTratamientoNavigation)
          .Include(t => t.idEnfermedadNavigation)
              .ThenInclude(e => e.idAnimalNavigation)
          .Where(t => t.idTipoTratamientoNavigation.retiroLecheDias != null
                      && t.idTipoTratamientoNavigation.retiroLecheDias.Value > 0)
          .Select(t => new
          {
            AnimalId = t.idEnfermedadNavigation.idAnimal,
            Codigo = t.idEnfermedadNavigation.idAnimalNavigation.codigo,
            Nombre = t.idEnfermedadNavigation.idAnimalNavigation.nombre,
            TipoTrat = t.idTipoTratamientoNavigation.nombre,
            Inicio = t.fechaInicio,
            UltimaDosis = (t.fechaFinalEstimada ?? t.fechaInicio),
            RetiroDias = t.idTipoTratamientoNavigation.retiroLecheDias!.Value
          })
          .ToListAsync();

      // calcula retiroHasta por tratamiento
      var calc = data.Select(x =>
      {
        var retiroHasta = x.UltimaDosis.Date
                  .AddDays(x.RetiroDias)
                  .AddDays(1)
                  .AddTicks(-1);

        return new
        {
          x.AnimalId,
          x.Codigo,
          x.Nombre,
          x.TipoTrat,
          x.Inicio,
          x.UltimaDosis,
          x.RetiroDias,
          RetiroHasta = retiroHasta
        };
      })
      // no mostrar tratamientos futuros
      .Where(x => x.Inicio.Date <= hoy)
      // solo los que caen en [hoy .. hoy+dias]
      .Where(x => x.RetiroHasta >= hoy && x.RetiroHasta <= hastaRango)
      .ToList();

      // agrupa por animal y elige el retiro más largo (más restrictivo)
      var items = calc
          .GroupBy(x => new { x.AnimalId, x.Codigo, x.Nombre })
          .Select(g =>
          {
            var top = g.OrderByDescending(z => z.RetiroHasta).First();
            var diasRest = (top.RetiroHasta.Date - hoy).Days;
            if (diasRest < 0) diasRest = 0;

            return new RetiroLecheItemViewModel
            {
              IdAnimal = g.Key.AnimalId,
              Codigo = string.IsNullOrWhiteSpace(g.Key.Codigo) ? "-" : g.Key.Codigo!,
              Nombre = string.IsNullOrWhiteSpace(g.Key.Nombre) ? "-" : g.Key.Nombre!,
              RetiroHasta = top.RetiroHasta,
              DiasRestantes = diasRest,
              TipoTratamiento = top.TipoTrat,
              UltimaDosis = top.UltimaDosis,
              RetiroDias = top.RetiroDias,
              InicioTratamiento = top.Inicio
            };
          })
          .OrderBy(x => x.RetiroHasta)
          .ThenBy(x => x.Codigo)
          .ToList();

      var vm = new RetiroLecheListadoViewModel
      {
        Dias = dias,
        Hoy = hoy,
        Items = items
      };

      return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var tratamiento = await _context.Tratamientos.FindAsync(id);
      if (tratamiento == null)
        return NotFound();

      _context.Tratamientos.Remove(tratamiento);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }
  }
}
