using WebZootecPro.Data;

namespace WebZootecPro.ViewModels.Costos
{
    public class CostosIndexVm
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? IdEstablo { get; set; }
        public int? IdHato { get; set; }     // = MovimientoCosto.IdCorral
        public int? IdAnimal { get; set; }
        public int? IdTipoCosto { get; set; }
        public int? IdCentroCosto { get; set; }

        public List<MovimientoCosto> Items { get; set; } = new();
        public decimal Total { get; set; }
    }
}
