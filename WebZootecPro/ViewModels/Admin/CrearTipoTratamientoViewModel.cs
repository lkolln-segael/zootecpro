using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebZootecPro.ViewModels.Admin
{
    public class CrearTipoTratamientoViewModel
    {
        public int? IdTipoTratamiento { get; set; }

        [Display(Name = "Nombre")]
        [Required]
        public string Nombre { get; set; } = null!;

        [Display(Name = "Costo")]
        [Required]
        public decimal Costo { get; set; } = 0;

        [Display(Name = "Cantidad")]
        [Required]
        public int Cantidad { get; set; } = 0;

        [Display(Name = "Unidad")]
        [Required]
        public string Unidad { get; set; } = null!;

        [Display(Name = "Retiro de leche (días)")]
        [Required]
        public int RetiroLecheDias { get; set; } = 0;


        [Display(Name = "IdTipoEnfermedad")]
        [Required]
        public int IdTipoEnfermedad { get; set; } = 0;

        public IEnumerable<SelectListItem>? Enfermedades { get; set; }
    }
}
