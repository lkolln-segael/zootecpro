using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RtmEntrega")]
[Index("fecha", "hatoId", Name = "IX_RtmEntrega_fecha_hato")]
public partial class RtmEntrega
{
    [Key]
    public int Id { get; set; }

    public int hatoId { get; set; }

    public int formulaId { get; set; }

    public DateOnly fecha { get; set; }

    [Precision(0)]
    public TimeOnly hora { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal kgTotal { get; set; }

    public int numeroVacas { get; set; }

    [Column(TypeName = "decimal(12, 4)")]
    public decimal kgPorVaca { get; set; }

    public int? idUsuario { get; set; }

    [StringLength(250)]
    public string? observacion { get; set; }

    [ForeignKey("formulaId")]
    [InverseProperty("RtmEntregas")]
    public virtual RtmFormula formula { get; set; } = null!;

    [ForeignKey("hatoId")]
    [InverseProperty("RtmEntregas")]
    public virtual Hato hato { get; set; } = null!;

    [ForeignKey("idUsuario")]
    [InverseProperty("RtmEntregas")]
    public virtual Usuario? idUsuarioNavigation { get; set; }
}
