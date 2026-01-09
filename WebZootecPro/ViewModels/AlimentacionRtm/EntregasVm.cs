namespace WebZootecPro.ViewModels.AlimentacionRtm
{
    public class EntregasVm
    {
        public int? hatoId { get; set; }
        public DateOnly fecha { get; set; }

        public List<EntregaRowVm> items { get; set; } = new();

        // Nuevo
        public int formulaId { get; set; }
        public TimeOnly hora { get; set; }
        public decimal kgTotal { get; set; }
        public int numeroVacas { get; set; }
        public string? observacion { get; set; }
    }

    public class EntregaRowVm
    {
        public int Id { get; set; }
        public string hato { get; set; } = "";
        public string formula { get; set; } = "";
        public DateOnly fecha { get; set; }
        public TimeOnly hora { get; set; }
        public decimal kgTotal { get; set; }
        public int numeroVacas { get; set; }
        public decimal kgPorVaca { get; set; }
        public decimal? costoKgFormula { get; set; }
        public decimal? costoTotal { get; set; }
    }
}
