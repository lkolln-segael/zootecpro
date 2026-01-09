using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Admin
{
  public class CrearPersonalViewModel
  {
    [Required]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = "VETERINARIO"; // VETERINARIO / INSPECTOR

    [Required]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;

    [Required]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Apellido")]
    public string? Apellido { get; set; }

    [Required]
    [Display(Name = "DNI")]
    public string DNI { get; set; } = null!;

    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Display(Name = "Provincia")]
    public string? Provincia { get; set; }

    [Display(Name = "Localidad")]
    public string? Localidad { get; set; }

    [Display(Name = "Código Postal")]
    public string? CodigoPostal { get; set; }

    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    public int? EmpresaId { get; set; }
    public List<SelectListItem>? Empresas { get; set; }

    [Required(ErrorMessage = "Seleccione el establo.")]
    public int? EstabloId { get; set; }
    public List<SelectListItem> Establos { get; set; } = new();

  }
}
