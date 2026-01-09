using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebZootecPro.ViewModels.Tratamientos
{
    public class CrearTratamientoViewModel
    {
        public int? IdTratamiento { get; set; }
        [Display(Name = "FechaInicio")]
        [Required]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "FechaFinalEstimada")]
        public DateTime? FechaFinalEstimada { get; set; }

        [Display(Name = "CostoEstimado")]
        [Required]
        public decimal CostoEstimado { get; set; }

        [Display(Name = "Observaciones")]
        [Required]
        public string Observaciones { get; set; } = null!;

        [Display(Name = "IdTipoTratamiento")]
        [Required]
        public int IdTipoTratamiento { get; set; }

        [Display(Name = "IdEnfermedad")]
        [Required]
        public int IdEnfermedad { get; set; }

        public int DiasEnEnfermeria { get; set; }
        public DateTime UltimaDosis { get; set; }
        public DateTime? RetiroHasta { get; set; }
        public bool EnRetiroHoy { get; set; }

        public IEnumerable<SelectListItem>? TipoTratamientos { get; set; }
        public IEnumerable<SelectListItem>? Enfermedades { get; set; }
    }
}
