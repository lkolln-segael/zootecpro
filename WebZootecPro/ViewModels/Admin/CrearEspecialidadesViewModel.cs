using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Admin
{
  public class CrearEspecialidadesViewModel
  {
    public int? Id { get; set; }
    [Display]
    [Required]
    public string Nombre { get; set; } = null!;
  }
}
