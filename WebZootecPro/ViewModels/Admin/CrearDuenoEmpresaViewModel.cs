using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Admin
{
    public class CrearDuenoEmpresaViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } = null!;

        [Required]
        [Display(Name = "Nombre completo")]
        public string NombrePersona { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string Dni { get; set; } = null!;

        // Datos de la empresa
        [Required]
        [Display(Name = "Nombre de la empresa")]
        public string NombreEmpresa { get; set; } = null!;

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 dígitos.")]
        [Display(Name = "RUC")]
        public string Ruc { get; set; } = null!;

        public int? CapacidadMaxima { get; set; }
        public decimal? AreaTotal { get; set; }
        public string? Ubicacion { get; set; }

        // Datos del dueño (colaborador)
        public string? Apellido { get; set; }
        public string? Direccion { get; set; }
        public string? Provincia { get; set; }
        public string? Localidad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Telefono { get; set; }


        [Required]
        [Display(Name = "Especialidad")]
        public int IdEspecialidad { get; set; }

        public List<SelectListItem> Especialidades { get; set; } = new();

        [Required]
        [Display(Name = "Plan de licencia")]
        public int? PlanId { get; set; }

        public List<SelectListItem> Planes { get; set; } = new();
    }
}
