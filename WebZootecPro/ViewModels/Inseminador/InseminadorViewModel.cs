
using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Inseminador;

public class InseminadorViewModel
{
  public int Id { get; set; }

  [Required]
  [StringLength(150)]
  [Display(Name = "Nombre")]
  public string Nombre { get; set; } = null!;

  [Required]
  [StringLength(150)]
  [Display(Name = "Apellido")]
  public string Apellido { get; set; } = null!;
}
