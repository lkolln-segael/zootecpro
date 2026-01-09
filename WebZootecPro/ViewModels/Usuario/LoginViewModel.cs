using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Usuario
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        public string? ReturnUrl { get; set; }
    }
}
