using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Medidores
{
    public class LecturaMedidorVm
    {
        public int? IdLecturaMedidorLeche { get; set; }

        [Required]
        public string CodigoMedidor { get; set; } = "";

        public string? CodigoAnimal { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FechaHoraLectura { get; set; } = DateTime.Now;

        [Required]
        [Range(0.001, 999999)]
        public decimal PesoLecheKg { get; set; }

        [Range(1, 10)]
        public byte NumeroOrdeno { get; set; } = 1;

        public bool Procesado { get; set; }
        public int? IdAnimal { get; set; }
        public int? IdRegistroProduccionLeche { get; set; }
        public string? Observacion { get; set; }
    }
}
