using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Enfermedades
{
    public class AltaEnfermeriaViewModel
    {
        [Required]
        public int IdEnfermedad { get; set; }

        [Display(Name = "Fecha de alta (recuperación)")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; } = DateTime.Today;
    }
}
