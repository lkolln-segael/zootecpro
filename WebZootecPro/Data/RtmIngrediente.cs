using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RtmIngrediente")]
public partial class RtmIngrediente
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string nombre { get; set; } = null!;

    [StringLength(20)]
    public string? unidad { get; set; }

    [Column(TypeName = "decimal(12, 4)")]
    public decimal? costoKg { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? msPct { get; set; }

    public bool activo { get; set; }

    [InverseProperty("ingrediente")]
    public virtual ICollection<RtmFormulaDetalle> RtmFormulaDetalles { get; set; } = new List<RtmFormulaDetalle>();
}
