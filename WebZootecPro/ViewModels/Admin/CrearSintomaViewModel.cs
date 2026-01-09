using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebZootecPro.ViewModels.Admin
{
  public class CrearSintomaViewModel
  {
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [Required]
    [Display(Name = "TipoEnfermedad")]
    public int IdEnfermedad { get; set; } = 1;

    public IEnumerable<SelectListItem>? Enfermedades { get; set; }
  }
}
