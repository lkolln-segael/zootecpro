using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RtmFormula")]
public partial class RtmFormula
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string nombre { get; set; } = null!;

    [StringLength(300)]
    public string? descripcion { get; set; }

    public bool activo { get; set; }

    [Precision(0)]
    public DateTime fechaCreacion { get; set; }

    [Column(TypeName = "decimal(12, 4)")]
    public decimal? costoKg { get; set; }

    [InverseProperty("formula")]
    public virtual ICollection<RtmEntrega> RtmEntregas { get; set; } = new List<RtmEntrega>();

    [InverseProperty("formula")]
    public virtual ICollection<RtmFormulaDetalle> RtmFormulaDetalles { get; set; } = new List<RtmFormulaDetalle>();

    [InverseProperty("formula")]
    public virtual ICollection<RtmRacionCorral> RtmRacionCorrals { get; set; } = new List<RtmRacionCorral>();
}
