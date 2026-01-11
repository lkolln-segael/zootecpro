using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Produccion
{
    public class RegistrarProduccionViewModel
    {
        [Required]
        [Display(Name = "Animal")]
        public int? IdAnimal { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha ordeño")]
        public DateTime FechaOrdeno { get; set; }

        [Required]
        [Display(Name = "Fuente")]
        public string Fuente { get; set; } = "GLORIA";


        [Required]
        [Display(Name = "Turno")]
        public string Turno { get; set; } = "MAÑANA";

        // ====== FASES DEL ORDEÑO (horas) ======
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Hora preparación")]
        public TimeSpan? HoraPreparacion { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Hora limpieza")]
        public TimeSpan? HoraLimpieza { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Hora despunte")]
        public TimeSpan? HoraDespunte { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Hora colocación pezoneras")]
        public TimeSpan? HoraColocacionPezoneras { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Hora inicio ordeño")]
        public TimeSpan? HoraInicio { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Hora fin ordeño")]
        public TimeSpan? HoraFin { get; set; }

        // ====== CANTIDADES ======
        [Required]
        [Display(Name = "Cantidad total (L)")]
        public decimal CantidadTotal { get; set; }

        [Display(Name = "A industria (L)")]
        public decimal CantidadIndustria { get; set; }

        [Display(Name = "A terneros (L)")]
        public decimal CantidadTerneros { get; set; }

        [Display(Name = "Descartada (L)")]
        public decimal CantidadDescartada { get; set; }

        [Display(Name = "Venta directa (L)")]
        public decimal CantidadVentaDirecta { get; set; }

        [Display(Name = "Tiene antibiótico")]
        public bool TieneAntibiotico { get; set; }

        [Display(Name = "Motivo descarte")]
        public string? MotivoDescarte { get; set; }

        [Display(Name = "Días en leche (DEL)")]
        public int? DiasEnLeche { get; set; }

        //nuevos campos
        [Display(Name = "% Grasa")]
        public decimal? Grasa { get; set; }

        [Display(Name = "% Proteína")]
        public decimal? Proteina { get; set; }

        [Display(Name = "% Sólidos Totales")]
        public decimal? SolidosTotales { get; set; }

        [Display(Name = "Urea")]
        public decimal? Urea { get; set; }

        [Display(Name = "RCS")]
        public int? Rcs { get; set; }

    }
}
