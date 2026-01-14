namespace WebZootecPro.ViewModels.Reportes
{
    public class ControlLecheroDiarioVm
    {
        public DateTime Fecha { get; set; }
        public int? HatoId { get; set; }
        public List<ControlLecheroDiarioItemVm> Items { get; set; } = new();
    }

    public class ControlLecheroDiarioItemVm
    {
        public int No { get; set; }
        public int AnimalId { get; set; }
        public string? Arete { get; set; }
        public string Nombre { get; set; } = "";
        public decimal AM { get; set; }
        public decimal PM { get; set; }
        public decimal Total => AM + PM;
    }
}
