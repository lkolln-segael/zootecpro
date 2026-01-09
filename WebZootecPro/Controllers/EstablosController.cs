using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
    public class EstablosController : Controller
    {
        private readonly ZootecContext _context;

        public EstablosController(ZootecContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin => User.IsInRole("SUPERADMIN");

        private int? GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idStr, out var id))
                return id;
            return null;
        }

        /// <summary>
        /// Empresas visibles:
        /// - SUPERADMIN: todas
        /// - ADMIN_EMPRESA: solo Empresa.usuarioID == usuario logueado
        /// </summary>
        private IQueryable<Empresa> QueryEmpresasVisibles()
        {
            if (IsSuperAdmin)
                return _context.Empresas.AsNoTracking();

            var userId = GetCurrentUserId();
            if (userId == null)
                return _context.Empresas.Where(_ => false).AsNoTracking();

            return _context.Empresas
                .Where(e => e.usuarioID == userId.Value)
                .AsNoTracking();
        }

        /// <summary>
        /// Establos visibles:
        /// - SUPERADMIN: todos
        /// - ADMIN_EMPRESA: solo establos cuya Empresa.usuarioID == usuario logueado
        /// </summary>
        private IQueryable<Establo> QueryEstablosVisibles()
        {
            if (IsSuperAdmin)
                return _context.Establos.AsNoTracking();

            var userId = GetCurrentUserId();
            if (userId == null)
                return _context.Establos.Where(_ => false).AsNoTracking();

            return _context.Establos
                .Join(
                    _context.Empresas,
                    est => est.EmpresaId,
                    emp => emp.Id,
                    (est, emp) => new { est, emp })
                .Where(x => x.emp.usuarioID == userId.Value)
                .Select(x => x.est)
                .AsNoTracking();
        }

        private async Task<bool> EmpresaEsVisibleAsync(int empresaId)
        {
            if (IsSuperAdmin) return true;

            var userId = GetCurrentUserId();
            if (userId == null) return false;

            return await _context.Empresas
                .AnyAsync(e => e.Id == empresaId && e.usuarioID == userId.Value);
        }

        private async Task<string?> ValidarLimiteEstablosAsync(int empresaId)
        {
            var emp = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new { e.PlanId })
                .FirstOrDefaultAsync();

            if (emp?.PlanId == null) return null; // sin plan => no limitamos aquí

            // Ajusta el nombre del DbSet según scaffold
            var maxEstablos = await _context.PlanLicencia
                .Where(p => p.Id == emp.PlanId.Value && p.Activo)
                .Select(p => p.MaxEstablos)
                .FirstOrDefaultAsync();

            if (maxEstablos == null) return null; // ilimitado

            var actuales = await _context.Establos.CountAsync(e => e.EmpresaId == empresaId);

            if (actuales >= maxEstablos.Value)
                return $"Límite alcanzado: tu plan permite {maxEstablos.Value} establos. Actualmente tienes {actuales}.";

            return null;
        }


        // GET: Establos
        public async Task<IActionResult> Index()
        {
            var establos = await QueryEstablosVisibles().ToListAsync();

            ViewBag.Empresas = await QueryEmpresasVisibles()
                .ToDictionaryAsync(e => e.Id, e => e.NombreEmpresa);

            return View(establos);
        }

        // GET: Establos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var establo = await QueryEstablosVisibles()
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (establo == null) return NotFound();

            return View(establo);
        }

        // GET: Establos/Create
        public async Task<IActionResult> Create()
        {
            var empresas = await QueryEmpresasVisibles()
                .OrderBy(e => e.NombreEmpresa)
                .ToListAsync();

            ViewBag.IdEmpresa = new SelectList(empresas, "Id", "NombreEmpresa");

            var model = new Establo();

            // Si es ADMIN_EMPRESA y solo tiene 1 empresa, la fijamos automático (sin dropdown)
            if (!IsSuperAdmin && empresas.Count == 1)
            {
                model.EmpresaId = empresas[0].Id;
                ViewBag.EmpresaUnica = true;
                ViewBag.EmpresaUnicaNombre = empresas[0].NombreEmpresa;
            }
            else
            {
                ViewBag.EmpresaUnica = false;
            }

            if (!IsSuperAdmin && empresas.Count == 0)
            {
                ModelState.AddModelError(string.Empty,
                    "No tienes empresas registradas. Crea una empresa antes de registrar un establo.");
            }

            return View(model);
        }

        // POST: Establos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Establo establo)
        {
            ModelState.Remove(nameof(Establo.Empresa));
            ModelState.Remove(nameof(Establo.Usuarios));
            ModelState.Remove(nameof(Establo.Hatos));

            // Default y validación simple
            establo.pveDias ??= 60;
            if (establo.pveDias < 0 || establo.pveDias > 365)
                ModelState.AddModelError(nameof(establo.pveDias), "P.V.E debe estar entre 0 y 365 días.");


            if (establo.EmpresaId <= 0)
                ModelState.AddModelError(nameof(establo.EmpresaId), "Debe seleccionar una empresa.");

            // Seguridad: que no pueda postear una EmpresaId de otro usuario
            if (establo.EmpresaId > 0 && !await EmpresaEsVisibleAsync(establo.EmpresaId))
                ModelState.AddModelError(nameof(establo.EmpresaId), "Empresa inválida para este usuario.");

            if (!ModelState.IsValid)
            {
                var empresas = await QueryEmpresasVisibles()
                    .OrderBy(e => e.NombreEmpresa)
                    .ToListAsync();

                ViewBag.IdEmpresa = new SelectList(empresas, "Id", "NombreEmpresa", establo.EmpresaId);

                if (!IsSuperAdmin && empresas.Count == 1)
                {
                    ViewBag.EmpresaUnica = true;
                    ViewBag.EmpresaUnicaNombre = empresas[0].NombreEmpresa;
                    establo.EmpresaId = empresas[0].Id;
                }
                else
                {
                    ViewBag.EmpresaUnica = false;
                }

                return View(establo);
            }

            var errorLicencia = await ValidarLimiteEstablosAsync(establo.EmpresaId);
            if (errorLicencia != null)
            {
                ModelState.AddModelError(string.Empty, errorLicencia);

                var empresas = await QueryEmpresasVisibles()
                    .OrderBy(e => e.NombreEmpresa)
                    .ToListAsync();

                ViewBag.IdEmpresa = new SelectList(empresas, "Id", "NombreEmpresa", establo.EmpresaId);
                ViewBag.EmpresaUnica = (!IsSuperAdmin && empresas.Count == 1);
                if (ViewBag.EmpresaUnica) ViewBag.EmpresaUnicaNombre = empresas[0].NombreEmpresa;

                return View(establo);
            }


            try
            {
                _context.Establos.Add(establo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Error al guardar establo: {ex.InnerException?.Message ?? ex.Message}");

                var empresas = await QueryEmpresasVisibles()
                    .OrderBy(e => e.NombreEmpresa)
                    .ToListAsync();

                ViewBag.IdEmpresa = new SelectList(empresas, "Id", "NombreEmpresa", establo.EmpresaId);

                if (!IsSuperAdmin && empresas.Count == 1)
                {
                    ViewBag.EmpresaUnica = true;
                    ViewBag.EmpresaUnicaNombre = empresas[0].NombreEmpresa;
                    establo.EmpresaId = empresas[0].Id;
                }
                else
                {
                    ViewBag.EmpresaUnica = false;
                }

                return View(establo);
            }
        }

        // GET: Establos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var establo = await QueryEstablosVisibles()
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (establo == null) return NotFound();

            var empresas = await QueryEmpresasVisibles()
                .OrderBy(e => e.NombreEmpresa)
                .ToListAsync();

            ViewBag.IdEmpresa = new SelectList(empresas, "Id", "NombreEmpresa", establo.EmpresaId);
            ViewBag.EmpresaSoloLectura = !IsSuperAdmin;

            if (!IsSuperAdmin)
                ViewBag.EmpresaNombreActual = empresas.FirstOrDefault(x => x.Id == establo.EmpresaId)?.NombreEmpresa ?? "-";

            return View(establo);
        }

        // POST: Establos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Establo establo)
        {
            ModelState.Remove(nameof(Establo.Empresa));
            ModelState.Remove(nameof(Establo.Usuarios));
            ModelState.Remove(nameof(Establo.Hatos));

            if (id != establo.Id) return NotFound();

            // Seguridad: el establo debe ser visible para el usuario
            var establoDb = await QueryEstablosVisibles()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (establoDb == null) return NotFound();

            // ADMIN_EMPRESA: NO puede cambiar EmpresaId (se fuerza al original)
            if (!IsSuperAdmin)
            {
                establo.EmpresaId = establoDb.EmpresaId;
            }
            else
            {
                if (!await _context.Empresas.AnyAsync(e => e.Id == establo.EmpresaId))
                    ModelState.AddModelError(nameof(establo.EmpresaId), "Empresa inválida.");
            }

            if (!ModelState.IsValid)
            {
                var empresas = await QueryEmpresasVisibles()
                    .OrderBy(e => e.NombreEmpresa)
                    .ToListAsync();

                ViewBag.IdEmpresa = new SelectList(empresas, "Id", "NombreEmpresa", establo.EmpresaId);
                ViewBag.EmpresaSoloLectura = !IsSuperAdmin;

                if (!IsSuperAdmin)
                    ViewBag.EmpresaNombreActual = empresas.FirstOrDefault(x => x.Id == establo.EmpresaId)?.NombreEmpresa ?? "-";

                return View(establo);
            }

            establo.pveDias ??= 60;
            if (establo.pveDias < 0 || establo.pveDias > 365)
            {
                ModelState.AddModelError(nameof(establo.pveDias), "P.V.E debe estar entre 0 y 365 días.");
            }

            _context.Update(establo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Establos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var establo = await QueryEstablosVisibles()
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (establo == null) return NotFound();

            return View(establo);
        }

        // POST: Establos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var establo = await QueryEstablosVisibles()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (establo == null) return NotFound();

            _context.Establos.Remove(establo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
