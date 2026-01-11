using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,LABORATORIO_EMPRESA,USUARIO_EMPRESA")]
    public class CalidadDiariaHatoController : Controller
    {
        private readonly ZootecContext _context;

        public CalidadDiariaHatoController(ZootecContext context)
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
        public async Task<IActionResult> Index(int? hatoId, DateTime? desde, DateTime? hasta, string? fuente)
        {
            var empresa = await GetEmpresaAsync();
            var establo = await _context.Establos.FirstOrDefaultAsync(e => e.EmpresaId == empresa.Id);
            var establoScope = establo.Id;
            var hatos = _context.Hatos.AsNoTracking()
            .Include(h => h.Establo).AsQueryable();
            hatos = hatos.Where(h => h.EstabloId == establoScope);

            var hatosList = await hatos
                .OrderBy(h => h.nombre)
                .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.nombre })
                .ToListAsync();

            ViewBag.Hatos = hatosList;

            var d1 = (desde ?? DateTime.Today.AddDays(-30)).Date;
            var d2 = (hasta ?? DateTime.Today).Date;

            var q = _context.CalidadDiariaHatos
                .AsNoTracking()
                .Include(x => x.idHatoNavigation)
                .Where(x => x.fecha >= DateOnly.FromDateTime(d1) && x.fecha <= DateOnly.FromDateTime(d2));

            if (hatoId.HasValue)
                q = q.Where(x => x.idHato == hatoId.Value);

            if (!string.IsNullOrWhiteSpace(fuente))
                q = q.Where(x => x.fuente == fuente);

            // Scope establo
            q = q.Where(x => x.idHatoNavigation.EstabloId == establoScope);

            var data = await q.OrderByDescending(x => x.fecha).ToListAsync();

            ViewBag.Desde = d1.ToString("yyyy-MM-dd");
            ViewBag.Hasta = d2.ToString("yyyy-MM-dd");
            ViewBag.HatoId = hatoId;
            ViewBag.Fuente = fuente ?? "";

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var empresa = await GetEmpresaAsync();
            var establo = await _context.Establos.FirstOrDefaultAsync(e => e.EmpresaId == empresa.Id);
            var establoScope = establo.Id;

            var hatos = _context.Hatos.AsNoTracking();
            hatos = hatos.Where(h => h.EstabloId == establoScope);

            ViewBag.Hatos = await hatos
                .OrderBy(h => h.nombre)
                .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.nombre })
                .ToListAsync();

            CargarComboFuentes("GLORIA");

            return View(new CalidadDiariaHato
            {
                fecha = DateOnly.FromDateTime(DateTime.Today),
                fuente = "GLORIA"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CalidadDiariaHato model)
        {
            // CLAVE: no validar navegación (no viene del form)
            ModelState.Remove(nameof(CalidadDiariaHato.idHatoNavigation));

            var establoScope = await GetEstabloScopeAsync();
            if (establoScope.HasValue)
            {
                var ok = await _context.Hatos.AsNoTracking()
                    .AnyAsync(h => h.Id == model.idHato && h.EstabloId == establoScope.Value);

                if (!ok) return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var hatos = _context.Hatos.AsNoTracking();
                if (establoScope.HasValue)
                    hatos = hatos.Where(h => h.EstabloId == establoScope.Value);

                ViewBag.Hatos = await hatos
                    .OrderBy(h => h.nombre)
                    .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.nombre })
                    .ToListAsync();
                CargarComboFuentes(model.fuente);

                return View(model);
            }

            // útil: setear fechaRegistro si tu BD lo usa
            model.fechaRegistro = DateTime.Now;

            _context.CalidadDiariaHatos.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void CargarComboFuentes(string? selected = null)
        {
            var fuentes = new List<SelectListItem>
              {
                new SelectListItem { Value = "GLORIA", Text = "GLORIA" },
                new SelectListItem { Value = "LAIVE",  Text = "LAIVE"  },
                new SelectListItem { Value = "OTRA",   Text = "OTRA"   },
              };

            ViewBag.Fuentes = new SelectList(fuentes, "Value", "Text", selected);
        }


    }
}
