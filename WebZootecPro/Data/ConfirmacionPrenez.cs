using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("ConfirmacionPrenez")]
[Index("idHato", Name = "IX_ConfirmacionPrenez_idHato")]
[Index("idRegistroReproduccion", Name = "IX_ConfirmacionPrenez_idRegistroReproduccion")]
public partial class ConfirmacionPrenez
{
    [Key]
    public int Id { get; set; }

    [StringLength(20)]
    public string tipo { get; set; } = null!;

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    public int idRegistroReproduccion { get; set; }

    [StringLength(600)]
    public string? observacion { get; set; }

    [StringLength(20)]
    public string? metodo { get; set; }

    public int? idHato { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("ConfirmacionPrenezs")]
    public virtual Hato? idHatoNavigation { get; set; }

    [ForeignKey("idRegistroReproduccion")]
    [InverseProperty("ConfirmacionPrenezs")]
    public virtual RegistroReproduccion idRegistroReproduccionNavigation { get; set; } = null!;
}
