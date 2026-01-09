using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Admin
{
    public class EditarPersonalViewModel
    {
        public int Id { get; set; }

        [ValidateNever]
        public string Tipo { get; set; } = null!;

        [ValidateNever]
        public string UserName { get; set; } = null!;

        [Required]
        public string Nombre { get; set; } = null!;

        public string? Apellido { get; set; }

        [Required]
        public string DNI { get; set; } = null!;

        public string? Direccion { get; set; }
        public string? Provincia { get; set; }
        public string? Localidad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Seleccione el establo.")]
        public int? EstabloId { get; set; }
        public List<SelectListItem> Establos { get; set; } = new();
    }
}
