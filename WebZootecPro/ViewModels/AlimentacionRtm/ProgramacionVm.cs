namespace WebZootecPro.ViewModels.AlimentacionRtm
{
    public class ProgramacionVm
    {
        public int? hatoId { get; set; }
        public List<ProgramacionRowVm> items { get; set; } = new();

        // Nuevo
        public int formulaId { get; set; }
        public TimeOnly hora { get; set; }

        public decimal kgRtmPorVaca { get; set; }
        public string? observacion { get; set; }
    }

    public class ProgramacionRowVm
    {
        public int Id { get; set; }
        public string hato { get; set; } = "";
        public string formula { get; set; } = "";
        public TimeOnly hora { get; set; }
        public decimal kgRtmPorVaca { get; set; }
        public bool activo { get; set; }
    }
}
