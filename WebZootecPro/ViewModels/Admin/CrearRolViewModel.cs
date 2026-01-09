using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Admin
{
  public class CrearRolViewModel
  {
    public int? Id { get; set; }
    [Required]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;
  }
}
