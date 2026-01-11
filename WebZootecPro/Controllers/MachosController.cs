using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA,INSPECTOR,VETERINARIO")]
    public class MachosController : Controller
    {
        private readonly ZootecContext _context;
        private bool IsSuperAdmin => User.IsInRole("SUPERADMIN");

        public MachosController(ZootecContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : null;
        }

        private async Task<Empresa?> GetEmpresaAsync()
        {
            var usuarioId = GetCurrentUserId();
            if (usuarioId == null) return null;

            return await _context.Empresas.FirstOrDefaultAsync(e => e.usuarioID == usuarioId
                || e.Colaboradors.Select(c => c.idUsuario).Contains(usuarioId.Value));
        }

        private async Task<IQueryable<Hato>> QueryHatosVisiblesAsync()
        {
            if (IsSuperAdmin)
                return _context.Hatos.AsNoTracking();

            var empresa = await GetEmpresaAsync();
            if (empresa == null)
                return _context.Hatos.AsNoTracking().Where(h => false);

            return _context.Hatos.AsNoTracking().Where(h => h.Establo.EmpresaId == empresa.Id);
        }

        private async Task CargarCombosAsync(Animal? animal = null, int? excludeAnimalId = null)
        {
            var hatosQ = await QueryHatosVisiblesAsync();

            ViewBag.IdHato = new SelectList(
                await hatosQ.OrderBy(h => h.nombre).ToListAsync(),
                "Id", "nombre", animal?.idHato
            );

            ViewBag.EstadoId = new SelectList(
                await _context.EstadoAnimals.AsNoTracking().OrderBy(e => e.nombre).ToListAsync(),
                "Id", "nombre", animal?.estadoId
            );

            ViewBag.PropositoId = new SelectList(
                await _context.PropositoAnimals.AsNoTracking().OrderBy(p => p.nombre).ToListAsync(),
                "Id", "nombre", animal?.propositoId
            );

            ViewBag.ProcedenciaId = new SelectList(
                await _context.ProcedenciaAnimals.AsNoTracking().OrderBy(p => p.nombre).ToListAsync(),
                "Id", "nombre", animal?.procedenciaId
            );

            ViewBag.IdRaza = new SelectList(
                await _context.Razas.AsNoTracking().OrderBy(r => r.nombre).ToListAsync(),
                "Id", "nombre", animal?.idRaza
            );

            ViewBag.EstadoProductivoId = new SelectList(
                await _context.EstadoProductivos.AsNoTracking().OrderBy(e => e.nombre).ToListAsync(),
                "Id", "nombre", animal?.estadoProductivoId
            );

            ViewBag.IdCategoriaAnimal = new SelectList(
                await _context.CategoriaAnimals.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(),
                "IdCategoriaAnimal", "Nombre", animal?.IdCategoriaAnimal
            );

            var padres = await _context.Animals.AsNoTracking()
                .Where(a => a.sexo != null && a.sexo.ToUpper() == "MACHO")
                .Where(a => excludeAnimalId == null || a.Id != excludeAnimalId)
                .OrderBy(a => a.codigo)
                .Select(a => new { a.Id, Label = (a.codigo ?? "-") + " - " + (a.nombre ?? "-") })
                .ToListAsync();

            ViewBag.IdPadre = new SelectList(padres, "Id", "Label", animal?.idPadre);
        }

        private void LimpiarValidacionNavegaciones()
        {
            ModelState.Remove(nameof(Animal.idHatoNavigation));
            ModelState.Remove(nameof(Animal.idPadreNavigation));
            ModelState.Remove(nameof(Animal.idMadreNavigation));
            ModelState.Remove(nameof(Animal.idUltimoCrecimientoNavigation));
            ModelState.Remove(nameof(Animal.estado));
            ModelState.Remove(nameof(Animal.proposito));
            ModelState.Remove(nameof(Animal.procedencia));
            ModelState.Remove(nameof(Animal.idRazaNavigation));
            ModelState.Remove(nameof(Animal.estadoProductivo));
            ModelState.Remove(nameof(Animal.Alimentacions));
            ModelState.Remove(nameof(Animal.DesarrolloCrecimientos));
            ModelState.Remove(nameof(Animal.Enfermedads));
            ModelState.Remove(nameof(Animal.InverseidMadreNavigation));
            ModelState.Remove(nameof(Animal.InverseidPadreNavigation));
            ModelState.Remove(nameof(Animal.PrenezidMadreAnimalNavigations));
            ModelState.Remove(nameof(Animal.PrenezidPadreAnimalNavigations));
            ModelState.Remove(nameof(Animal.RegistroIngresos));
            ModelState.Remove(nameof(Animal.RegistroNacimientos));
            ModelState.Remove(nameof(Animal.RegistroProduccionLeches));
            ModelState.Remove(nameof(Animal.RegistroReproduccions));
            ModelState.Remove(nameof(Animal.RegistroSalida));
            ModelState.Remove(nameof(Animal.TipoAlimentos));
        }

        public async Task<IActionResult> Index()
        {
            var hatosQ = await QueryHatosVisiblesAsync();
            var hatoIds = await hatosQ.Select(h => h.Id).ToListAsync();

            var list = await _context.Animals
                .AsNoTracking()
                .Include(a => a.idHatoNavigation)
                .Where(a => hatoIds.Contains(a.idHato))
                .Where(a => a.sexo != null && a.sexo.ToUpper() == "MACHO")
                .OrderBy(a => a.codigo)
                .ThenBy(a => a.nombre)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            var animal = new Animal { sexo = "MACHO", nacimientoEstimado = false };
            await CargarCombosAsync(animal);
            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Animal animal)
        {
            LimpiarValidacionNavegaciones();

            animal.sexo = "MACHO";
            animal.nombre = (animal.nombre ?? "").Trim();
            animal.codigo = (animal.codigo ?? "").Trim();
            animal.arete = (animal.arete ?? "").Trim();

            var hatosQ = await QueryHatosVisiblesAsync();
            var hatoOk = await hatosQ.AnyAsync(h => h.Id == animal.idHato);
            if (!hatoOk) ModelState.AddModelError(nameof(animal.idHato), "Hato inválido.");

            if (string.IsNullOrWhiteSpace(animal.nombre))
                ModelState.AddModelError(nameof(animal.nombre), "Ingrese nombre.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(animal);
                return View(animal);
            }

            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var hatosQ = await QueryHatosVisiblesAsync();
            var hatoIds = await hatosQ.Select(h => h.Id).ToListAsync();

            var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && hatoIds.Contains(a.idHato));
            if (animal == null) return NotFound();

            animal.sexo = "MACHO";
            await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Animal animal)
        {
            if (id != animal.Id) return NotFound();

            LimpiarValidacionNavegaciones();

            var hatosQ = await QueryHatosVisiblesAsync();
            var hatoOk = await hatosQ.AnyAsync(h => h.Id == animal.idHato);
            if (!hatoOk) ModelState.AddModelError(nameof(animal.idHato), "Hato inválido.");

            animal.sexo = "MACHO";
            animal.nombre = (animal.nombre ?? "").Trim();
            animal.codigo = (animal.codigo ?? "").Trim();
            animal.arete = (animal.arete ?? "").Trim();

            if (string.IsNullOrWhiteSpace(animal.nombre))
                ModelState.AddModelError(nameof(animal.nombre), "Ingrese nombre.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(animal, excludeAnimalId: animal.Id);
                return View(animal);
            }

            _context.Update(animal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var hatosQ = await QueryHatosVisiblesAsync();
            var hatoIds = await hatosQ.Select(h => h.Id).ToListAsync();

            var animal = await _context.Animals
                .AsNoTracking()
                .Include(a => a.idHatoNavigation)
                .FirstOrDefaultAsync(a => a.Id == id && hatoIds.Contains(a.idHato));

            if (animal == null) return NotFound();
            return View(animal);
        }

        private async Task CargarHatosAsync(int establoScope)
        {
            ViewBag.Hatos = await _context.Hatos.AsNoTracking()
              .Where(h => h.EstabloId == establoScope)
              .OrderBy(h => h.nombre)
              .Select(h => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
              {
                  Value = h.Id.ToString(),
                  Text = h.nombre
              })
              .ToListAsync();
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hatosQ = await QueryHatosVisiblesAsync();
            var hatoIds = await hatosQ.Select(h => h.Id).ToListAsync();

            var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && hatoIds.Contains(a.idHato));
            if (animal == null) return NotFound();

            _context.Animals.Remove(animal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
