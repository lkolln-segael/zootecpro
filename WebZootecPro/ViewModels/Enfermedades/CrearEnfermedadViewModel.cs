using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebZootecPro.ViewModels.Enfermedades
{
  public class CrearEnfermedadViewModel
  {
        [Display(Name = "IdEnfermedad")]
        public int? IdEnfermedad { get; set; }

        [Display(Name = "Fecha diagnóstico")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaDiagnostico { get; set; }

        [Display(Name = "Fecha recuperación (alta)")]
        [DataType(DataType.Date)]
        public DateTime? FechaRecuperacion { get; set; }  // ✅ ahora nullable (baja/alta formal)

        [Display(Name = "Veterinario")]
        [Required]
        public int IdVeterinario { get; set; }

        [Display(Name = "Tipo de enfermedad")]
        [Required]
        public int IdTipoEnfermedad { get; set; }

        [Display(Name = "Animal")]
        [Required]
        public int IdAnimal { get; set; }

        // ✅ para mostrar en Index (sin cálculo en la vista)
        public int DiasEnEnfermeria { get; set; }
        public bool EnEnfermeria { get; set; }

        public IEnumerable<SelectListItem>? Animales { get; set; }
        public IEnumerable<SelectListItem>? Veterinarios { get; set; }
        public IEnumerable<SelectListItem>? TipoEnfermedades { get; set; }

    }
}
