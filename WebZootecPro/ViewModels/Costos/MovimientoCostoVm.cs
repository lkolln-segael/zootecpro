using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Costos
{
    public class MovimientoCostoVm
    {
        public int? IdMovimientoCosto { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required]
        public int IdCentroCosto { get; set; }

        [Required]
        public int IdTipoCosto { get; set; }

        [Required]
        [Range(0.01, 999999999)]
        public decimal MontoTotal { get; set; }

        public string? Descripcion { get; set; }

        // Ojo: en tu BD "corral" realmente lo estás usando como Hato
        public int? IdEstablo { get; set; }
        public int? IdCorral { get; set; }  // = Hato.Id
        public int? IdAnimal { get; set; }
        public int? IdRegistroProduccionLeche { get; set; }

        // combos
        public List<SelectListItem> CentrosCosto { get; set; } = new();
        public List<SelectListItem> TiposCosto { get; set; } = new();
        public List<SelectListItem> Establos { get; set; } = new();
        public List<SelectListItem> Hatos { get; set; } = new();
        public List<SelectListItem> Animales { get; set; } = new();
    }
}
