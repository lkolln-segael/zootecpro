using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Reportes;
using System.Linq;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA,USUARIO_EMPRESA")]
    public class ReportesController : Controller
    {
        private readonly ZootecContext _context;

        public ReportesController(ZootecContext context)
        {
            _context = context;
        }

        // =========================
        // Helpers de usuario/scope
        // =========================
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

        private IQueryable<Animal> BuildAnimalesScopeQuery(Usuario? u, int? idHatoFiltro)
        {
            var q = _context.Animals
                .AsNoTracking()
                .Include(a => a.estado)
                .Include(a => a.estadoProductivo)
                .Include(a => a.idHatoNavigation)
                .AsQueryable();

            if (u?.idHato != null)
                q = q.Where(a => a.idHato == u.idHato.Value);
            else if (u?.idEstablo != null)
                q = q.Where(a => a.idHatoNavigation.EstabloId == u.idEstablo.Value);

            if (idHatoFiltro != null)
                q = q.Where(a => a.idHato == idHatoFiltro.Value);

            return q;
        }

        private async Task CargarHatosAsync(Usuario? u, int? idHatoSeleccionado)
        {
            var hatosQ = _context.Hatos.AsNoTracking().AsQueryable();

            if (u?.idEstablo != null)
                hatosQ = hatosQ.Where(h => h.EstabloId == u.idEstablo.Value);

            if (u?.idHato != null)
                hatosQ = hatosQ.Where(h => h.Id == u.idHato.Value);

            var hatos = await hatosQ.OrderBy(h => h.nombre).ToListAsync();
            ViewBag.IdHato = new SelectList(hatos, "Id", "nombre", idHatoSeleccionado);
        }

        private static int GetMinDiasConfirmacionPrenez(string? metodo)
        {
            return (metodo ?? "").Trim().ToUpperInvariant() switch
            {
                "ECOGRAFIA" => 35,
                "PALPACION" => 60,
                _ => 35
            };
        }

        private async Task<int?> GetEstadoProductivoIdAsync(string nombre)
        {
            return await _context.EstadoProductivos
                .AsNoTracking()
                .Where(x => x.nombre == nombre)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        // =====================================================
        // 0) Landing
        // =====================================================
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ReproduccionKpi));
        }

        // =====================================================
        // 1) KPI REPRODUCCIÓN
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> ReproduccionKpi(DateOnly? desde, DateOnly? hasta, int? idHato)
        {
            var u = await GetUsuarioActualAsync();
            await CargarHatosAsync(u, idHato);

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var fDesde = desde ?? hoy.AddDays(-30);
            var fHasta = hasta ?? hoy;

            if (fHasta < fDesde)
            {
                // swap simple
                var tmp = fDesde;
                fDesde = fHasta;
                fHasta = tmp;
            }

            var scopeAnimalesQ = BuildAnimalesScopeQuery(u, idHato);

            // Base: hembras activas (para KPI)
            var hembrasActivasQ = scopeAnimalesQ.Where(a =>
                (a.sexo ?? "").ToUpper() == "HEMBRA" &&
                (a.estado == null || a.estado.nombre != "INACTIVO")
            );

            var totalHembrasActivas = await hembrasActivasQ.CountAsync();

            // Rango DateTime (inclusive)
            var dDesde = fDesde;   // DateOnly
            var dHasta = fHasta;   // DateOnly

            var dtDesde = fDesde.ToDateTime(TimeOnly.MinValue);               // DateTime (para fechaRegistro)
            var dtHasta = fHasta.ToDateTime(new TimeOnly(23, 59, 59));        // DateTime (para fechaRegistro)

            var PVE_DIAS = await GetPveDiasAsync(u, idHato);

            var asOf = dtHasta;      // DateTime
            var asOfDate = dHasta;   // DateOnly


            // Ultimo parto <= asOf
            var ultParto = await _context.Partos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { rr.idAnimal, p.fechaRegistro })
                .Where(x => hembrasActivasQ.Select(a => a.Id).Contains(x.idAnimal) && x.fechaRegistro <= asOf)
                .GroupBy(x => x.idAnimal)
                .Select(g => new { idAnimal = g.Key, fecha = g.Max(x => x.fechaRegistro) })
                .ToDictionaryAsync(x => x.idAnimal, x => (DateTime?)x.fecha);

            // Ultimo servicio (inseminación) <= asOf
            var ultServicio = await _context.Prenezs.AsNoTracking()
      .Where(p => p.idMadreAnimal != null && p.fechaInseminacion != null
          && p.fechaInseminacion.Value <= asOfDate)
      .Where(p => hembrasActivasQ.Select(a => a.Id).Contains(p.idMadreAnimal!.Value))
      .GroupBy(p => p.idMadreAnimal!.Value)
      .Select(g => new { idAnimal = g.Key, fecha = g.Max(x => x.fechaInseminacion) })
      .ToDictionaryAsync(x => x.idAnimal, x => (DateOnly?)x.fecha);


            // Preñadas al corte (confirmación POSITIVA <= asOf y sin parto/aborto <= asOf)
            var preñadasSet = (await (
      from c in _context.ConfirmacionPrenezs.AsNoTracking()
      join rr in _context.RegistroReproduccions.AsNoTracking() on c.idRegistroReproduccion equals rr.Id
      where hembrasActivasQ.Select(a => a.Id).Contains(rr.idAnimal)
            && c.tipo == "POSITIVA"
            && c.fechaRegistro <= asOf
            && !_context.Abortos.Any(a => a.idRegistroReproduccion == c.idRegistroReproduccion && a.fechaRegistro <= asOf)
            && !_context.Partos.Any(p => p.idRegistroReproduccion == c.idRegistroReproduccion && p.fechaRegistro <= asOf)
      select rr.idAnimal
  ).Distinct().ToListAsync()).ToHashSet();


            var secaId = await GetEstadoProductivoIdAsync("SECA");

            // Elegibles = hembras activas, no SECA, no preñadas al corte, y cumplen PVE al corte
            var hembrasActivas = await hembrasActivasQ.ToListAsync();

            int elegiblesAlCorte = 0;
            foreach (var a in hembrasActivas)
            {
                if (secaId != null && a.estadoProductivoId == secaId.Value) continue;
                if (preñadasSet.Contains(a.Id)) continue;

                DateTime? refDt = null;
                if (ultParto.TryGetValue(a.Id, out var up) && up != null) refDt = up.Value.Date;

                if (ultServicio.TryGetValue(a.Id, out var us) && us != null)
                {
                    var usDt = us.Value.ToDateTime(TimeOnly.MinValue).Date;
                    refDt = refDt == null ? usDt : (usDt > refDt.Value ? usDt : refDt);
                }

                if (refDt != null)
                {
                    var min = refDt.Value.AddDays(PVE_DIAS);
                    if (asOf.Date < min.Date) continue;
                }

                elegiblesAlCorte++;
            }

            // ---------- Servicios (inseminaciones) en periodo ----------
            var serviciosEnPeriodo = await _context.Prenezs.AsNoTracking()
                .Where(p => p.idMadreAnimal != null && p.fechaInseminacion != null)
               .Where(p => p.fechaInseminacion!.Value >= dDesde && p.fechaInseminacion!.Value <= dHasta)

                .Where(p => hembrasActivasQ.Select(a => a.Id).Contains(p.idMadreAnimal!.Value))
                .CountAsync();

            // ---------- Concepciones (servicios en periodo con confirmación POSITIVA en cualquier fecha posterior) ----------
            var concepcionesEnPeriodo = await (
                from p in _context.Prenezs.AsNoTracking()
                where p.idMadreAnimal != null && p.fechaInseminacion != null
                let fi = p.fechaInseminacion!.Value
                where fi >= dDesde && fi <= dHasta

                where hembrasActivasQ.Select(a => a.Id).Contains(p.idMadreAnimal!.Value)
                where _context.ConfirmacionPrenezs.Any(c => c.idRegistroReproduccion == p.idRegistroReproduccion && c.tipo == "POSITIVA")
                select p.idRegistroReproduccion
            ).Distinct().CountAsync();

            // ---------- Concepción 1er servicio (en periodo) ----------
            var totalPrimerServicio = await _context.Prenezs.AsNoTracking()
                .Where(p => p.idMadreAnimal != null && p.fechaInseminacion != null && p.numeroServicio == 1)
                .Where(p => p.fechaInseminacion!.Value >= dDesde && p.fechaInseminacion!.Value <= dHasta)

                .Where(p => hembrasActivasQ.Select(a => a.Id).Contains(p.idMadreAnimal!.Value))
                .CountAsync();

            var concepcionPrimerServicio = await (
                from p in _context.Prenezs.AsNoTracking()
                where p.idMadreAnimal != null && p.fechaInseminacion != null && p.numeroServicio == 1
                let fi = p.fechaInseminacion!.Value
                where fi >= dDesde && fi <= dHasta

                where hembrasActivasQ.Select(a => a.Id).Contains(p.idMadreAnimal!.Value)
                where _context.ConfirmacionPrenezs.Any(c => c.idRegistroReproduccion == p.idRegistroReproduccion && c.tipo == "POSITIVA")
                select p.idRegistroReproduccion
            ).Distinct().CountAsync();

            // ---------- Días al 1er servicio (post-parto) ----------
            // Para lactación actual: último parto <= corte, primer servicio después de ese parto
            var partosAll = await _context.Partos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { rr.idAnimal, p.fechaRegistro })
                .Where(x => hembrasActivasQ.Select(a => a.Id).Contains(x.idAnimal) && x.fechaRegistro <= asOf)
                .OrderBy(x => x.idAnimal).ThenBy(x => x.fechaRegistro)
                .ToListAsync();

            var serviciosAll = await _context.Prenezs.AsNoTracking()
                .Where(p => p.idMadreAnimal != null && p.fechaInseminacion != null)
                .Where(p => hembrasActivasQ.Select(a => a.Id).Contains(p.idMadreAnimal!.Value))
                .Select(p => new
                {
                    AnimalId = p.idMadreAnimal!.Value,
                    FechaInsem = p.fechaInseminacion!.Value,
                    RrId = p.idRegistroReproduccion
                })
                .ToListAsync();

            var partosPorAnimal = partosAll
                .GroupBy(x => x.idAnimal)
                .ToDictionary(g => g.Key, g => g.Select(x => x.fechaRegistro).OrderBy(d => d).ToList());

            var serviciosPorAnimal = serviciosAll
                .GroupBy(x => x.AnimalId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.FechaInsem).ToList());

            // Días al primer servicio (servicio cae en periodo)
            var diasPrimerServicioList = new List<int>();
            foreach (var a in hembrasActivas)
            {
                if (!partosPorAnimal.TryGetValue(a.Id, out var partosList) || partosList.Count == 0) continue;

                var ultimoPartoDt = partosList.Last(); // último parto <= corte
                if (!serviciosPorAnimal.TryGetValue(a.Id, out var servList)) continue;

                var primerServPostParto = servList
                    .Where(s => s.FechaInsem.ToDateTime(TimeOnly.MinValue) >= ultimoPartoDt.Date)
                    .Select(s => s.FechaInsem)
                    .FirstOrDefault();

                if (primerServPostParto == default) continue;

                var fsDt = primerServPostParto.ToDateTime(TimeOnly.MinValue);
                if (fsDt < dtDesde || fsDt > dtHasta) continue; // cae fuera del periodo

                var dias = (fsDt.Date - ultimoPartoDt.Date).Days;
                if (dias >= 0) diasPrimerServicioList.Add(dias);
            }

            double? diasPrimerServicioProm = diasPrimerServicioList.Count > 0 ? diasPrimerServicioList.Average() : null;

            // ---------- Días abiertos (post-parto a concepción) ----------
            var diasAbiertosList = new List<int>();
            foreach (var a in hembrasActivas)
            {
                if (!partosPorAnimal.TryGetValue(a.Id, out var partosList) || partosList.Count == 0) continue;

                var ultimoPartoDt = partosList.Last();
                if (!serviciosPorAnimal.TryGetValue(a.Id, out var servList)) continue;

                // Concepción = primer servicio postparto que tenga confirmación POSITIVA
                var concepcion = servList
                    .Where(s => s.FechaInsem.ToDateTime(TimeOnly.MinValue) >= ultimoPartoDt.Date)
                    .Where(s => _context.ConfirmacionPrenezs.AsNoTracking().Any(c => c.idRegistroReproduccion == s.RrId && c.tipo == "POSITIVA"))
                    .Select(s => s.FechaInsem)
                    .FirstOrDefault();

                if (concepcion == default) continue;

                var ccDt = concepcion.ToDateTime(TimeOnly.MinValue);
                if (ccDt < dtDesde || ccDt > dtHasta) continue; // concepción en periodo

                var dias = (ccDt.Date - ultimoPartoDt.Date).Days;
                if (dias >= 0) diasAbiertosList.Add(dias);
            }

            double? diasAbiertosProm = diasAbiertosList.Count > 0 ? diasAbiertosList.Average() : null;

            // ---------- Intervalo entre partos (si el parto cae en periodo) ----------
            var intervaloPartosList = new List<int>();
            foreach (var kv in partosPorAnimal)
            {
                var fechas = kv.Value;
                if (fechas.Count < 2) continue;

                for (int i = 1; i < fechas.Count; i++)
                {
                    var actual = fechas[i];
                    if (actual < dtDesde || actual > dtHasta) continue;

                    var anterior = fechas[i - 1];
                    var dias = (actual.Date - anterior.Date).Days;
                    if (dias > 0) intervaloPartosList.Add(dias);
                }
            }

            double? intervaloEntrePartosProm = intervaloPartosList.Count > 0 ? intervaloPartosList.Average() : null;

            // ---------- % preñadas a 150/200 DIM (al corte) ----------
            int base150 = 0, preg150 = 0, base200 = 0, preg200 = 0;

            foreach (var a in hembrasActivas)
            {
                if (!partosPorAnimal.TryGetValue(a.Id, out var partosList) || partosList.Count == 0) continue;
                var ultimoPartoDt = partosList.Last();

                var dim = (asOf.Date - ultimoPartoDt.Date).Days;
                if (dim < 0) continue;

                DateTime? fechaConcepcion = null;
                if (serviciosPorAnimal.TryGetValue(a.Id, out var servList))
                {
                    var conc = servList
                        .Where(s => s.FechaInsem.ToDateTime(TimeOnly.MinValue) >= ultimoPartoDt.Date)
                        .Where(s => _context.ConfirmacionPrenezs.AsNoTracking().Any(c => c.idRegistroReproduccion == s.RrId && c.tipo == "POSITIVA"))
                        .Select(s => s.FechaInsem.ToDateTime(TimeOnly.MinValue))
                        .FirstOrDefault();

                    if (conc != default) fechaConcepcion = conc;
                }

                if (dim >= 150)
                {
                    base150++;
                    if (fechaConcepcion != null && fechaConcepcion.Value.Date <= ultimoPartoDt.Date.AddDays(150)) preg150++;
                }

                if (dim >= 200)
                {
                    base200++;
                    if (fechaConcepcion != null && fechaConcepcion.Value.Date <= ultimoPartoDt.Date.AddDays(200)) preg200++;
                }
            }

            // ---------- Tasa aborto (abortos en periodo / confirmaciones positivas en periodo) ----------
            var abortosEnPeriodo = await _context.Abortos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    a => a.idRegistroReproduccion,
                    rr => rr.Id,
                    (a, rr) => new { rr.idAnimal, a.fechaRegistro })
                .Where(x => hembrasActivasQ.Select(an => an.Id).Contains(x.idAnimal))
                .Where(x => x.fechaRegistro >= dtDesde && x.fechaRegistro <= dtHasta)
                .CountAsync();

            var confirmPositivasEnPeriodo = await _context.ConfirmacionPrenezs.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    c => c.idRegistroReproduccion,
                    rr => rr.Id,
                    (c, rr) => new { rr.idAnimal, c.tipo, c.fechaRegistro })
                .Where(x => hembrasActivasQ.Select(an => an.Id).Contains(x.idAnimal))
                .Where(x => x.tipo == "POSITIVA" && x.fechaRegistro >= dtDesde && x.fechaRegistro <= dtHasta)
                .CountAsync();

            // ---------- KPIs finales ----------
            decimal pct(decimal num, decimal den) => den == 0 ? 0 : Math.Round((num / den) * 100m, 2);

            var vm = new ReproduccionKpiViewModel
            {
                Desde = fDesde,
                Hasta = fHasta,
                IdHato = idHato,

                TotalHembrasActivas = totalHembrasActivas,
                VacasElegiblesAlCorte = elegiblesAlCorte,

                ServiciosEnPeriodo = serviciosEnPeriodo,
                ConcepcionesEnPeriodo = concepcionesEnPeriodo,

                TasaPrenez = pct(concepcionesEnPeriodo, elegiblesAlCorte),
                TasaInseminacion = pct(serviciosEnPeriodo, elegiblesAlCorte),

                TotalPrimerServicio = totalPrimerServicio,
                ConcepcionPrimerServicio = concepcionPrimerServicio,
                ConcepcionPrimerServicioPct = pct(concepcionPrimerServicio, totalPrimerServicio),

                ServiciosPorConcepcion = concepcionesEnPeriodo == 0 ? 0 : Math.Round((decimal)serviciosEnPeriodo / concepcionesEnPeriodo, 2),

                DiasPrimerServicioProm = diasPrimerServicioProm,
                DiasAbiertosProm = diasAbiertosProm,
                IntervaloEntrePartosProm = intervaloEntrePartosProm,

                Base150DIM = base150,
                Prenadas150DIM = preg150,
                PorcPrenadas150DIM = pct(preg150, base150),

                Base200DIM = base200,
                Prenadas200DIM = preg200,
                PorcPrenadas200DIM = pct(preg200, base200),

                AbortosEnPeriodo = abortosEnPeriodo,
                ConfirmPositivasEnPeriodo = confirmPositivasEnPeriodo,
                TasaAbortoPct = pct(abortosEnPeriodo, confirmPositivasEnPeriodo)
            };

            return View(vm);
        }

        // =====================================================
        // 2) LISTADO: Elegibles a inseminar (y estados)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> ElegiblesInseminacion(DateOnly? fechaCorte, int pveDias = 60, int? idHato = null)
        {
            var u = await GetUsuarioActualAsync();
            await CargarHatosAsync(u, idHato);

            var corte = fechaCorte ?? DateOnly.FromDateTime(DateTime.Today);
            var asOf = corte.ToDateTime(new TimeOnly(23, 59, 59));

            if (pveDias < 0) pveDias = 0;
            if (pveDias > 365) pveDias = 365;

            var scopeAnimalesQ = BuildAnimalesScopeQuery(u, idHato);

            var hembrasActivas = await scopeAnimalesQ
                .Where(a => (a.sexo ?? "").ToUpper() == "HEMBRA" && (a.estado == null || a.estado.nombre != "INACTIVO"))
                .ToListAsync();

            var secaId = await GetEstadoProductivoIdAsync("SECA");

            // Último parto <= corte
            var ultParto = await _context.Partos.AsNoTracking()
                .Join(_context.RegistroReproduccions.AsNoTracking(),
                    p => p.idRegistroReproduccion,
                    rr => rr.Id,
                    (p, rr) => new { rr.idAnimal, p.fechaRegistro })
                .Where(x => hembrasActivas.Select(a => a.Id).Contains(x.idAnimal) && x.fechaRegistro <= asOf)
                .GroupBy(x => x.idAnimal)
                .Select(g => new { idAnimal = g.Key, fecha = g.Max(x => x.fechaRegistro) })
                .ToDictionaryAsync(x => x.idAnimal, x => (DateTime?)x.fecha);

            // Último servicio <= corte
            var asOfDate = corte; // DateOnly
           // var asOf = corte.ToDateTime(new TimeOnly(23, 59, 59)); // DateTime (para Parto/Confirmación)

            var ultServicio = await _context.Prenezs.AsNoTracking()
                .Where(p => p.idMadreAnimal != null && p.fechaInseminacion != null)
                .Where(p => hembrasActivas.Select(a => a.Id).Contains(p.idMadreAnimal!.Value))
                .Where(p => p.fechaInseminacion!.Value <= asOfDate)
                .GroupBy(p => p.idMadreAnimal!.Value)
                .Select(g => new { idAnimal = g.Key, fecha = g.Max(x => x.fechaInseminacion) })
                .ToDictionaryAsync(x => x.idAnimal, x => (DateOnly?)x.fecha);


            // Última confirmación <= corte (para estado "vacía" / "preñada")
            var confData = await (
                from c in _context.ConfirmacionPrenezs.AsNoTracking()
                join rr in _context.RegistroReproduccions.AsNoTracking() on c.idRegistroReproduccion equals rr.Id
                where hembrasActivas.Select(a => a.Id).Contains(rr.idAnimal)
                      && c.fechaRegistro <= asOf
                select new { rr.idAnimal, c.tipo, c.metodo, c.fechaRegistro }
            ).ToListAsync();

            var ultConfirm = confData
                .GroupBy(x => x.idAnimal)
                .Select(g => g.OrderByDescending(x => x.fechaRegistro).First())
                .ToDictionary(x => x.idAnimal, x => x);

            // Preñadas al corte (confirmación POSITIVA <= corte y sin parto/aborto <= corte)
            var preñadasSet = (await (
     from c in _context.ConfirmacionPrenezs.AsNoTracking()
     join rr in _context.RegistroReproduccions.AsNoTracking() on c.idRegistroReproduccion equals rr.Id
     where hembrasActivas.Select(a => a.Id).Contains(rr.idAnimal)
           && c.tipo == "POSITIVA"
           && c.fechaRegistro <= asOf
           && !_context.Abortos.Any(a => a.idRegistroReproduccion == c.idRegistroReproduccion && a.fechaRegistro <= asOf)
           && !_context.Partos.Any(p => p.idRegistroReproduccion == c.idRegistroReproduccion && p.fechaRegistro <= asOf)
     select rr.idAnimal
 ).Distinct().ToListAsync()).ToHashSet();


            var rows = new List<ElegibleInseminacionRowVm>();

            foreach (var a in hembrasActivas.OrderBy(x => x.codigo))
            {
                var isSeca = (secaId != null && a.estadoProductivoId == secaId.Value);
                var isPreñada = preñadasSet.Contains(a.Id);

                ultParto.TryGetValue(a.Id, out var up);
                ultServicio.TryGetValue(a.Id, out var us);

                DateTime? refDt = null;
                if (up != null) refDt = up.Value.Date;
                if (us != null)
                {
                    var usDt = us.Value.ToDateTime(TimeOnly.MinValue).Date;
                    refDt = refDt == null ? usDt : (usDt > refDt.Value ? usDt : refDt);
                }

                DateTime? fechaMin = null;
                int? diasDesdeRef = null;
                if (refDt != null)
                {
                    fechaMin = refDt.Value.AddDays(pveDias);
                    diasDesdeRef = (asOf.Date - refDt.Value.Date).Days;
                }

                // Confirmación última
                string? ultTipo = null, ultMetodo = null;
                DateTime? ultFecha = null;
                if (ultConfirm.TryGetValue(a.Id, out var uc))
                {
                    ultTipo = uc.tipo;
                    ultMetodo = uc.metodo;
                    ultFecha = uc.fechaRegistro;
                }

                // Estado lógico
                string estado;
                if (isSeca) estado = "SECA";
                else if (isPreñada) estado = "PREÑADA";
                else if (refDt != null && fechaMin != null && asOf.Date < fechaMin.Value.Date) estado = "NO CUMPLE PVE";
                else
                {
                    // “Vacía confirmada” si la última confirmación fue NEGATIVA
                    if ((ultTipo ?? "").ToUpper() == "NEGATIVA") estado = "ELEGIBLE (VACÍA)";
                    else if (us != null && ultTipo == null) estado = "PENDIENTE DIAGNÓSTICO";
                    else if (us == null) estado = "ELEGIBLE (SIN SERVICIO)";
                    else estado = "ELEGIBLE";
                }

                rows.Add(new ElegibleInseminacionRowVm
                {
                    AnimalId = a.Id,
                    Codigo = a.codigo ?? "-",
                    Nombre = a.nombre ?? "-",
                    Hato = a.idHatoNavigation?.nombre ?? "-",
                    EstadoProductivo = a.estadoProductivo?.nombre ?? "-",

                    UltimoParto = up,
                    UltimaInseminacion = us,

                    UltimaConfirmacionTipo = ultTipo,
                    UltimaConfirmacionMetodo = ultMetodo,
                    UltimaConfirmacionFecha = ultFecha,

                    FechaReferencia = refDt,
                    FechaMinimaInseminar = fechaMin,
                    DiasDesdeReferencia = diasDesdeRef,

                    Estado = estado
                });
            }

            var vm = new ElegiblesInseminacionViewModel
            {
                FechaCorte = corte,
                PveDias = pveDias,
                IdHato = idHato,
                Items = rows
            };

            return View(vm);
        }

        // =====================================================
        // 3) LISTADO: Vacas para revisión (35 eco / 60 palpación)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> RevisionPrenez(string metodo = "ECOGRAFIA", DateOnly? desde = null, DateOnly? hasta = null, int? idHato = null)
        {
            var u = await GetUsuarioActualAsync();
            await CargarHatosAsync(u, idHato);

            var m = (metodo ?? "ECOGRAFIA").Trim().ToUpperInvariant();
            if (m != "ECOGRAFIA" && m != "PALPACION") m = "ECOGRAFIA";

            var fDesde = desde ?? DateOnly.FromDateTime(DateTime.Today);
            var fHasta = hasta ?? fDesde.AddDays(7);
            if (fHasta < fDesde) { var tmp = fDesde; fDesde = fHasta; fHasta = tmp; }

            var minDias = GetMinDiasConfirmacionPrenez(m);

            var scopeAnimalesQ = BuildAnimalesScopeQuery(u, idHato);

            // Solo hembras activas
            var hembrasActivasQ = scopeAnimalesQ.Where(a =>
                (a.sexo ?? "").ToUpper() == "HEMBRA" &&
                (a.estado == null || a.estado.nombre != "INACTIVO")
            );

            // Servicios cuya "fecha de revisión" cae dentro del rango
            // revisión = fechaInseminacion + minDias
            var data = await (
                from p in _context.Prenezs.AsNoTracking()
                join rr in _context.RegistroReproduccions.AsNoTracking() on p.idRegistroReproduccion equals rr.Id
                join a in _context.Animals.AsNoTracking().Include(x => x.idHatoNavigation) on rr.idAnimal equals a.Id
                where p.idMadreAnimal != null && p.fechaInseminacion != null
                where hembrasActivasQ.Select(x => x.Id).Contains(a.Id)

                // excluir si ya tiene confirmación registrada
                where !_context.ConfirmacionPrenezs.Any(c => c.idRegistroReproduccion == p.idRegistroReproduccion)

                // excluir si ya terminó por aborto/parto
                where !_context.Abortos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)
                where !_context.Partos.Any(x => x.idRegistroReproduccion == p.idRegistroReproduccion)

                select new
                {
                    AnimalId = a.Id,
                    a.codigo,
                    a.nombre,
                    Hato = a.idHatoNavigation.nombre,
                    FechaInsem = p.fechaInseminacion!.Value,
                    p.numeroServicio,
                    p.nombreToro,
                    p.codigoNaab,
                    p.protocolo
                }
            ).ToListAsync();

            var rows = new List<RevisionPrenezRowVm>();
            foreach (var x in data)
            {
                var rev = x.FechaInsem.AddDays(minDias);
                if (rev < fDesde || rev > fHasta) continue;

                var diasDesde = (DateOnly.FromDateTime(DateTime.Today).DayNumber - x.FechaInsem.DayNumber);

                rows.Add(new RevisionPrenezRowVm
                {
                    AnimalId = x.AnimalId,
                    Codigo = x.codigo ?? "-",
                    Nombre = x.nombre ?? "-",
                    Hato = x.Hato ?? "-",
                    FechaInseminacion = x.FechaInsem,
                    FechaRevision = rev,
                    DiasDesdeInseminacion = diasDesde < 0 ? 0 : diasDesde,
                    NumeroServicio = x.numeroServicio,
                    NombreToro = x.nombreToro,
                    CodigoNaab = x.codigoNaab,
                    Protocolo = x.protocolo
                });
            }

            rows = rows.OrderBy(r => r.FechaRevision).ThenBy(r => r.Codigo).ToList();

            var vm = new RevisionPrenezViewModel
            {
                Metodo = m,
                MinDias = minDias,
                Desde = fDesde,
                Hasta = fHasta,
                IdHato = idHato,
                Items = rows
            };

            return View(vm);
        }

        // GET: /Reportes/Enfermeria
        [HttpGet]
        public async Task<IActionResult> Enfermeria(int? soloActivos)
        {
            var q = _context.vw_TratamientosEnfermeria.AsQueryable();

            if (soloActivos == 1)
                q = q.Where(x => x.EstaEnEnfermeria == 1);

            var items = await q
                .OrderByDescending(x => x.EstaEnEnfermeria)
                .ThenByDescending(x => x.DiasEnEnfermeria)
                .Take(500)
                .ToListAsync();

            ViewBag.SoloActivos = soloActivos == 1;
            return View(items);
        }

        private async Task<int> GetPveDiasAsync(Usuario? u, int? idHato)
        {
            int? establoId = null;

            if (idHato != null)
            {
                establoId = await _context.Hatos
                    .Where(h => h.Id == idHato.Value)
                    .Select(h => (int?)h.EstabloId)
                    .FirstOrDefaultAsync();
            }
            else if (u?.idEstablo != null)
            {
                establoId = u.idEstablo.Value;
            }
            else if (u?.idHato != null)
            {
                establoId = await _context.Hatos
                    .Where(h => h.Id == u.idHato.Value)
                    .Select(h => (int?)h.EstabloId)
                    .FirstOrDefaultAsync();
            }

            if (establoId == null) return 60;

            var pve = await _context.Establos
                .Where(e => e.Id == establoId.Value)
                .Select(e => (int?)e.pveDias)
                .FirstOrDefaultAsync();

            return pve ?? 60;
        }

        // ===================================
        // Reporte: Producción diaria por Hato
        // ===================================
        [HttpGet]
        public async Task<IActionResult> ProduccionDiaria(int? hatoId, DateTime? desde, DateTime? hasta, string? turno)
        {
            var d1 = (desde ?? DateTime.Today.AddDays(-7)).Date;
            var d2 = (hasta ?? DateTime.Today).Date;

            // para incluir el día final completo
            var d2Fin = d2.AddDays(1);

            var q = _context.RegistroProduccionLeches
                .AsNoTracking()
                .Include(r => r.idAnimalNavigation)
                    .ThenInclude(a => a.idHatoNavigation)
                .Where(r => (r.fechaOrdeno ?? r.fechaRegistro) >= d1
         && (r.fechaOrdeno ?? r.fechaRegistro) < d2Fin);

            if (hatoId.HasValue)
                q = q.Where(r => r.idAnimalNavigation.idHato == hatoId.Value);

            if (!string.IsNullOrWhiteSpace(turno))
                q = q.Where(r => r.turno == turno);

            var items = await q
                .GroupBy(r => new
                {
                    Fecha = (r.fechaOrdeno ?? r.fechaRegistro).Date,
                    Turno = r.turno,
                    HatoId = r.idAnimalNavigation.idHato,
                    Hato = r.idAnimalNavigation.idHatoNavigation.nombre
                })
                .Select(g => new ProduccionDiariaItemVm
                {
                    Fecha = g.Key.Fecha,
                    Turno = g.Key.Turno,
                    HatoId = g.Key.HatoId,
                    Hato = g.Key.Hato,

                    Producido = g.Sum(x => x.pesoOrdeno ?? 0),
                    Industria = g.Sum(x => x.cantidadIndustria ?? 0),
                    Terneros = g.Sum(x => x.cantidadTerneros ?? 0),
                    Descartada = g.Sum(x => x.cantidadDescartada ?? 0),
                    VentaDirecta = g.Sum(x => x.cantidadVentaDirecta ?? 0),

                    VacasOrdeñadas = g.Select(x => x.idAnimal).Distinct().Count(),
                    Registros = g.Count()
                })
                .OrderByDescending(x => x.Fecha)
                .ThenBy(x => x.Hato)
                .ThenBy(x => x.Turno)
                .ToListAsync();

            var vm = new ProduccionDiariaVm
            {
                Desde = d1,
                Hasta = d2,
                HatoId = hatoId,
                Turno = turno,
                Items = items
            };

            // combos hatos
            var usuario = await GetUsuarioActualAsync();
            var hatos = _context.Hatos.AsNoTracking();
            if (usuario?.idEstablo != null)
                hatos = hatos.Where(h => h.EstabloId == usuario.idEstablo.Value);

            ViewBag.Hatos = await hatos
                .OrderBy(h => h.nombre)
                .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.nombre })
                .ToListAsync();

            return View(vm);
        }

        // ===================================
        // Reporte: Alimentación (Entregas)
        // ===================================
        [HttpGet]
        public async Task<IActionResult> AlimentacionResumen(int? hatoId, DateTime? desde, DateTime? hasta)
        {
            var d1 = (desde ?? DateTime.Today.AddDays(-7)).Date;
            var d2 = (hasta ?? DateTime.Today).Date;
            var d2Fin = d2.AddDays(1);

            var usuario = await GetUsuarioActualAsync();

            // Raciones activas (programado) por hato+formula
            var raciones = _context.RtmRacionCorrals.AsNoTracking().Where(r => r.activo);
            if (usuario?.idEstablo != null)
                raciones = raciones.Where(r => r.hato.EstabloId == usuario.idEstablo.Value);

            var racionDict = await raciones
                .Include(r => r.formula)
                .ToDictionaryAsync(
                    k => (k.hatoId, k.formulaId),
                    v => (decimal?)v.kgRtmPorVaca
                );

            var f1 = DateOnly.FromDateTime(d1);
            var f2Ex = DateOnly.FromDateTime(d2.AddDays(1));
            var entregas = _context.RtmEntregas
                .AsNoTracking()
                .Include(e => e.hato)
                .Include(e => e.formula)
                .Where(e => e.fecha >= f1 && e.fecha < f2Ex);

            if (usuario?.idEstablo != null)
                entregas = entregas.Where(e => e.hato.EstabloId == usuario.idEstablo.Value);

            if (hatoId.HasValue)
                entregas = entregas.Where(e => e.hatoId == hatoId.Value);

            var items = await entregas
                .OrderByDescending(e => e.fecha)
                .ThenBy(e => e.hato.nombre)
                .ThenBy(e => e.formula.nombre)
                .Select(e => new AlimentacionResumenItemVm
                {
                    Fecha = e.fecha,
                    Hato = e.hato.nombre,
                    Formula = e.formula.nombre,
                    KgTotal = e.kgTotal,
                    NumeroVacas = e.numeroVacas,
                    KgPorVaca = e.kgPorVaca
                })
                .ToListAsync();

            // completa programado/diferencia
            foreach (var it in items)
            {
                
            }

            // combos hatos
            var hatos = _context.Hatos.AsNoTracking();
            if (usuario?.idEstablo != null)
                hatos = hatos.Where(h => h.EstabloId == usuario.idEstablo.Value);

            ViewBag.Hatos = await hatos
                .OrderBy(h => h.nombre)
                .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.nombre })
                .ToListAsync();

            return View(new AlimentacionResumenVm
            {
                Desde = d1,
                Hasta = d2,
                HatoId = hatoId,
                Items = items
            });
        }

        // ===================================
        // Reporte: Costos (resumen)
        // ===================================
        [HttpGet]
        [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
        public async Task<IActionResult> CostosResumen(DateTime? desde, DateTime? hasta)
        {
            var d1 = (desde ?? DateTime.Today.AddDays(-30)).Date;
            var d2 = (hasta ?? DateTime.Today).Date;
            var d2Fin = d2.AddDays(1);

            var usuario = await GetUsuarioActualAsync();
            var f1 = DateOnly.FromDateTime(d1);
            var f2Ex = DateOnly.FromDateTime(d2.AddDays(1));
            var q = _context.MovimientoCostos
                .AsNoTracking()
                .Include(m => m.IdCentroCostoNavigation)
                .Include(m => m.IdTipoCostoNavigation)
                .Where(m => m.Fecha >= f1 && m.Fecha < f2Ex);

            if (usuario?.idEstablo != null)
                q = q.Where(m => m.IdEstablo == usuario.idEstablo.Value);

            var items = await q
                .GroupBy(m => new
                {
                    Anio = m.Fecha.Year,
                    Mes = m.Fecha.Month,
                    Centro = m.IdCentroCostoNavigation.Nombre,
                    Tipo = m.IdTipoCostoNavigation.Nombre
                })
                .Select(g => new CostosResumenItemVm
                {
                    Anio = g.Key.Anio,
                    Mes = g.Key.Mes,
                    CentroCosto = g.Key.Centro,
                    TipoCosto = g.Key.Tipo,
                    MontoTotal = g.Sum(x => x.MontoTotal),
                    Movimientos = g.Count()
                })
                .OrderByDescending(x => x.Anio)
                .ThenByDescending(x => x.Mes)
                .ThenBy(x => x.CentroCosto)
                .ThenBy(x => x.TipoCosto)
                .ToListAsync();

            return View(new CostosResumenVm
            {
                Desde = d1,
                Hasta = d2,
                Items = items
            });
        }

    }
}
