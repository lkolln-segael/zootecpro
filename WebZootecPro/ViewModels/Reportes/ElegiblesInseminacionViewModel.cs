namespace WebZootecPro.ViewModels.Reportes
{
    public class ElegiblesInseminacionViewModel
    {
        public DateOnly FechaCorte { get; set; }
        public int PveDias { get; set; } = 60;
        public int? IdHato { get; set; }

        public List<ElegibleInseminacionRowVm> Items { get; set; } = new();
    }

    public class ElegibleInseminacionRowVm
    {
        public int AnimalId { get; set; }
        public string Codigo { get; set; } = "-";
        public string Nombre { get; set; } = "-";
        public string Hato { get; set; } = "-";
        public string EstadoProductivo { get; set; } = "-";

        public DateTime? UltimoParto { get; set; }
        public DateOnly? UltimaInseminacion { get; set; }

        public string? UltimaConfirmacionTipo { get; set; }
        public string? UltimaConfirmacionMetodo { get; set; }
        public DateTime? UltimaConfirmacionFecha { get; set; }

        public DateTime? FechaReferencia { get; set; }
        public DateTime? FechaMinimaInseminar { get; set; }
        public int? DiasDesdeReferencia { get; set; }

        public string Estado { get; set; } = "-";
    }
}
