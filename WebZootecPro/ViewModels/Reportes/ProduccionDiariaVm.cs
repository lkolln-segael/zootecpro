namespace WebZootecPro.ViewModels.Reportes
{
    public class ProduccionDiariaVm
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int? HatoId { get; set; }
        public string? Turno { get; set; }

        public List<ProduccionDiariaItemVm> Items { get; set; } = new();
    }

    public class ProduccionDiariaItemVm
    {
        public DateTime Fecha { get; set; }
        public string Turno { get; set; } = "";
        public int HatoId { get; set; }
        public string Hato { get; set; } = "";

        public decimal Producido { get; set; }
        public decimal Industria { get; set; }
        public decimal Terneros { get; set; }
        public decimal Descartada { get; set; }
        public decimal VentaDirecta { get; set; }

        public int VacasOrdeñadas { get; set; }
        public int Registros { get; set; }
    }
}
