namespace WebZootecPro.ViewModels.AlimentacionRtm
{
    public class IngredienteVm
    {
        public int Id { get; set; }
        public string nombre { get; set; } = "";
        public string? unidad { get; set; }
        public decimal? costoKg { get; set; }
        public decimal? msPct { get; set; }
        public bool activo { get; set; }
    }
}
