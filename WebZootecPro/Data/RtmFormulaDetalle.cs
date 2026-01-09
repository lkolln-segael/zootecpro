using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RtmFormulaDetalle")]
[Index("formulaId", Name = "IX_RtmFormulaDetalle_formulaId")]
public partial class RtmFormulaDetalle
{
    [Key]
    public int Id { get; set; }

    public int formulaId { get; set; }

    public int ingredienteId { get; set; }

    [Column(TypeName = "decimal(7, 4)")]
    public decimal porcentaje { get; set; }

    [StringLength(200)]
    public string? observacion { get; set; }

    [ForeignKey("formulaId")]
    [InverseProperty("RtmFormulaDetalles")]
    public virtual RtmFormula formula { get; set; } = null!;

    [ForeignKey("ingredienteId")]
    [InverseProperty("RtmFormulaDetalles")]
    public virtual RtmIngrediente ingrediente { get; set; } = null!;
}
