using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Produccion
{
    public class ReporteIndustriaFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required]
        [StringLength(20)]
        public string Turno { get; set; } = "MAÑANA";

        [Required]
        public int IdHato { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal PesoReportado { get; set; }

        [StringLength(200)]
        public string? Observacion { get; set; }
    }

    public class ComparativoIndustriaRowViewModel
    {
        public DateTime Fecha { get; set; }
        public string Turno { get; set; } = "";
        public string Hato { get; set; } = "";

        public decimal ProducidoTotal { get; set; }          // SUM(pesoOrdeno)
        public decimal EntregadoIndustria { get; set; }      // SUM(cantidadIndustria)
        public decimal? ReportadoIndustria { get; set; }     // tabla nueva

        public decimal DifProducidoVsEntregado => ProducidoTotal - EntregadoIndustria;

        public decimal? DifEntregadoVsReportado =>
            ReportadoIndustria.HasValue ? (EntregadoIndustria - ReportadoIndustria.Value) : null;
    }
}
