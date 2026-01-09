namespace WebZootecPro.ViewModels.AlimentacionRtm
{
    public class ConsumoVm
    {
        public int? hatoId { get; set; }
        public DateOnly desde { get; set; }
        public DateOnly hasta { get; set; }

        public List<ConsumoRowVm> items { get; set; } = new();
    }

    public class ConsumoRowVm
    {
        public DateOnly fecha { get; set; }
        public string hato { get; set; } = "";
        public decimal kgTotal { get; set; }
        public decimal? costoTotal { get; set; }
    }
}
