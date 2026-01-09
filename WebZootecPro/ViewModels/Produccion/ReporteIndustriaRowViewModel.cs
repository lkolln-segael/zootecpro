namespace WebZootecPro.ViewModels.Produccion
{
    public class ReporteIndustriaRowViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Turno { get; set; } = "";
        public int IdHato { get; set; }
        public string HatoNombre { get; set; } = "";

        public decimal PesoReportado { get; set; }
        public string? Observacion { get; set; }
    }
}
