using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RtmRacionCorral")]
[Index("hatoId", Name = "IX_RtmRacionCorral_hatoId")]
public partial class RtmRacionCorral
{
    [Key]
    public int Id { get; set; }

    public int hatoId { get; set; }

    public int formulaId { get; set; }

    [Precision(0)]
    public TimeOnly hora { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal kgRtmPorVaca { get; set; }

    public bool activo { get; set; }

    [StringLength(200)]
    public string? observacion { get; set; }

    [ForeignKey("formulaId")]
    [InverseProperty("RtmRacionCorrals")]
    public virtual RtmFormula formula { get; set; } = null!;

    [ForeignKey("hatoId")]
    [InverseProperty("RtmRacionCorrals")]
    public virtual Hato hato { get; set; } = null!;
}
