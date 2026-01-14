namespace WebZootecPro.ViewModels.Reportes
{
    public class PicoCampaniaVm
    {
        public DateTime FechaCorte { get; set; }
        public int? HatoId { get; set; }
        public List<PicoCampaniaItemVm> Items { get; set; } = new();
    }

    public class PicoCampaniaItemVm
    {
        public int No { get; set; }
        public string? Arete { get; set; }
        public string Nombre { get; set; } = "";
        public DateTime? Parto { get; set; }
        public int? dl { get; set; }
        public int? udl { get; set; }

        // 1..7
        public List<decimal?> Picos { get; set; } = new List<decimal?>(new decimal?[7]);
        public List<decimal?> Promedios { get; set; } = new List<decimal?>(new decimal?[7]);
    }
}
