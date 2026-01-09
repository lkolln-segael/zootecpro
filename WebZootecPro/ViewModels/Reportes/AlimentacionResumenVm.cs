namespace WebZootecPro.ViewModels.Reportes
{
    public class AlimentacionResumenVm
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int? HatoId { get; set; }

        public List<AlimentacionResumenItemVm> Items { get; set; } = new();
    }

    public class AlimentacionResumenItemVm
    {
        public DateOnly Fecha { get; set; }
        public string Hato { get; set; } = "";
        public string Formula { get; set; } = "";

        public decimal KgTotal { get; set; }
        public int NumeroVacas { get; set; }
        public decimal KgPorVaca { get; set; }

        // Programado (si existe ración activa)
        public decimal? KgPorVacaProgramado { get; set; }
        public decimal? DiferenciaKgPorVaca { get; set; }
    }
}
