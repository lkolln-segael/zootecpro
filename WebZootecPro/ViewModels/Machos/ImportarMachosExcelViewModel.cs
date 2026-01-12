using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Machos
{
    public class ImportarMachosExcelViewModel
    {

        [Required]
        [Display(Name = "Hato")]
        public int? IdHato { get; set; }

        [Required]
        [Display(Name = "Archivo Excel (.xlsx)")]
        public IFormFile? Archivo { get; set; }

    }
}
