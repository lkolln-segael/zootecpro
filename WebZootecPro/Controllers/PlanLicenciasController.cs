using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN")]
    public class PlanLicenciasController : Controller
    {
        private readonly ZootecContext _context;
        public PlanLicenciasController(ZootecContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var planes = await _context.PlanLicencia
              .AsNoTracking()
              .OrderByDescending(p => p.Activo)
              .ThenBy(p => p.Nombre)
              .ToListAsync();

            return View(planes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new PlanLicencium
            {
                Activo = true,
                Moneda = "PEN",
                Precio = 0,
                EsIndefinido = false,
                FechaRegistro = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanLicencium model)
        {
            if (!ModelState.IsValid) return View(model);

            model.FechaRegistro = DateTime.Now;

            _context.PlanLicencia.Add(model);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar. Verifica que el Código sea único.");
                return View(model);
            }

            TempData["PlanMessage"] = "Plan creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _context.PlanLicencia.FindAsync(id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlanLicencium model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var plan = await _context.PlanLicencia.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return NotFound();

            // actualizar campos
            plan.Codigo = model.Codigo;
            plan.Nombre = model.Nombre;
            plan.Precio = model.Precio;
            plan.Moneda = model.Moneda;
            plan.EsIndefinido = model.EsIndefinido;
            plan.MaxAnimales = model.MaxAnimales;
            plan.MaxEstablos = model.MaxEstablos;
            plan.Activo = model.Activo; // ✅ activar/desactivar aquí

            // NO tocar FechaRegistro
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar. Verifica que el Código sea único.");
                return View(model);
            }

            TempData["PlanMessage"] = "Plan actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
