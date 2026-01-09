using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.CampaniaLechera
{
  public class CampaniaLecheraModel
  {
    public int EstabloId { get; set; }

    [Required]
    public string nombre { get; set; } = null!;
    [Required]
    public DateOnly fechaInicio { get; set; }
    [Required]
    public DateOnly fechaFin { get; set; }
    [Required]
    public bool activa { get; set; } = false;

    public string? observaciones { get; set; }
  }
}
