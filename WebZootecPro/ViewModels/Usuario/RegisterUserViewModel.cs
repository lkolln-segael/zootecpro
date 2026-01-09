using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Usuario
{
    public class RegisterUserViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } = null!;

        [Required]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        // ---- Rol (id) ----
        [Required]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        // Si los usas:
        public int? IdEstablo { get; set; }
        public int? IdHato { get; set; }
    }
}
