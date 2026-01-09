namespace WebZootecPro.ViewModels.Reportes
{
    public class CostosResumenVm
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public List<CostosResumenItemVm> Items { get; set; } = new();
    }

    public class CostosResumenItemVm
    {
        public int Anio { get; set; }
        public int Mes { get; set; }

        public string CentroCosto { get; set; } = "";
        public string TipoCosto { get; set; } = "";

        public decimal MontoTotal { get; set; }
        public int Movimientos { get; set; }
    }
}
