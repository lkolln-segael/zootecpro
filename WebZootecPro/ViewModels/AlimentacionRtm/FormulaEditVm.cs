namespace WebZootecPro.ViewModels.AlimentacionRtm
{
    public class FormulaEditVm
    {
        public int Id { get; set; }
        public string nombre { get; set; } = "";
        public string? descripcion { get; set; }
        public bool activo { get; set; }
        public decimal? costoKg { get; set; }

        public decimal porcentajeTotal { get; set; }

        public List<FormulaDetalleRowVm> detalles { get; set; } = new();

        // Para agregar línea
        public int ingredienteId { get; set; }
        public decimal porcentaje { get; set; }
        public string? observacion { get; set; }
    }

    public class FormulaDetalleRowVm
    {
        public int Id { get; set; }
        public int ingredienteId { get; set; }
        public string ingrediente { get; set; } = "";
        public decimal porcentaje { get; set; }
        public decimal? costoKg { get; set; }
    }
}
