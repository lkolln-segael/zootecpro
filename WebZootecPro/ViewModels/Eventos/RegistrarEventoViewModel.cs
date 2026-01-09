using System.ComponentModel.DataAnnotations;

namespace WebZootecPro.ViewModels.Eventos
{
    public class RegistrarEventoViewModel
    {
        [Required]
        [Display(Name = "Animal")]
        public int? IdAnimal { get; set; }

        [Display(Name = "Hato")]
        public int? IdHato { get; set; }

        [Required]
        [Display(Name = "Fecha del evento")]
        [DataType(DataType.Date)]
        public DateOnly? FechaEvento { get; set; }

        [Required]
        [Display(Name = "Tipo de evento")]
        public string TipoEvento { get; set; } = null!;

        [Display(Name = "Estado productivo")]
        public int? EstadoProductivoId { get; set; }

        [Display(Name = "Descripción / observaciones")]
        public string? Observaciones { get; set; }

        // ====== PARTO ======

        [Display(Name = "Hora del parto")]
        [DataType(DataType.Time)]
        public TimeOnly? HoraParto { get; set; }

        [Display(Name = "Sexo de la cría")]
        public int? IdSexoCria { get; set; }

        [Display(Name = "Tipo de parto")]
        public int? IdTipoParto { get; set; }

        [Display(Name = "Estado de la cría")]
        public int? IdEstadoCria { get; set; }

        [Display(Name = "RP cría 1")]
        [StringLength(60)]
        public string? RpCria1 { get; set; }

        [Display(Name = "RP cría 2 (si mellizo)")]
        [StringLength(60)]
        public string? RpCria2 { get; set; }

        [Display(Name = "Nombre cría 1")]
        [StringLength(60)]
        public string? NombreCria1 { get; set; }

        [Display(Name = "Nombre cría 2 (si mellizo)")]
        [StringLength(60)]
        public string? NombreCria2 { get; set; }

        // ====== REPRODUCCIÓN / OTROS ======

        public int? IdCausaAborto { get; set; }

        [Display(Name = "Confirmación")]
        public string? ConfirmacionTipo { get; set; }

        [Display(Name = "Motivo de seca")]
        public string? SecaMotivo { get; set; }

        [Display(Name = "Padre (toro)")]
        public int? IdPadreAnimal { get; set; }

        // ====== ENFERMEDAD / MEDICACIÓN ======

        [Display(Name = "Tipo de enfermedad")]
        public int? IdTipoEnfermedad { get; set; }

        [Display(Name = "Veterinario")]
        public int? IdVeterinario { get; set; }

        [Display(Name = "Tipo de tratamiento")]
        public int? IdTipoTratamiento { get; set; }

        [Display(Name = "Costo estimado")]
        [DataType(DataType.Currency)]
        public decimal? CostoEstimado { get; set; }

        // Para MEDICACIÓN (caso seleccionado)
        public int? IdEnfermedad { get; set; }

        // ====== SALIDA ======

        [Display(Name = "Destino / comprador")]
        public string? DestinoSalida { get; set; }

        // ====== ANÁLISIS / TEXTO LIBRE ======

        [Display(Name = "Tipo de análisis")]
        public string? TipoAnalisis { get; set; }

        [Display(Name = "Resultado")]
        public string? Resultado { get; set; }

        // ====== SERVICIO / INSEMINACIÓN ======
        [Display(Name = "Hora de servicio")]
        [DataType(DataType.Time)]
        public TimeOnly? HoraServicio { get; set; }

        [Display(Name = "Nombre del toro")]
        [StringLength(150)]
        public string? NombreToro { get; set; }

        [Display(Name = "Código NAAB")]
        [StringLength(20)]
        public string? CodigoNaab { get; set; }

        [Display(Name = "Protocolo (OVYSINCH, CIDER, IATF...)")]
        [StringLength(50)]
        public string? Protocolo { get; set; }

        //confirmacion prenez
        [Display(Name = "Método")]
        public string? ConfirmacionMetodo { get; set; } // "ECOGRAFIA" | "PALPACION"

    }
}
