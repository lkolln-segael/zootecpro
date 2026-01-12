using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Seca")]
[Index("idHato", Name = "IX_Seca_idHato")]
[Index("idRegistroReproduccion", Name = "IX_Seca_idRegistroReproduccion")]
public partial class Seca
{
    [Key]
    public int Id { get; set; }

    [StringLength(250)]
    public string? motivo { get; set; }

    public int idRegistroReproduccion { get; set; }

    [Precision(0)]
    public DateTime? fechaSeca { get; set; }

    public int? diasSecaReal { get; set; }

    public int? idHato { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("Secas")]
    public virtual Hato? idHatoNavigation { get; set; }

    [ForeignKey("idRegistroReproduccion")]
    [InverseProperty("Secas")]
    public virtual RegistroReproduccion idRegistroReproduccionNavigation { get; set; } = null!;
}
