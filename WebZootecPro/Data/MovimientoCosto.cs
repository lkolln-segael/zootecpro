using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("MovimientoCosto")]
[Index("Fecha", Name = "IX_MovimientoCosto_Fecha")]
[Index("IdAnimal", Name = "IX_MovimientoCosto_IdAnimal")]
public partial class MovimientoCosto
{
    [Key]
    public int IdMovimientoCosto { get; set; }

    public DateOnly Fecha { get; set; }

    public int IdCentroCosto { get; set; }

    public int IdTipoCosto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MontoTotal { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public int? IdEstablo { get; set; }

    public int? IdCorral { get; set; }

    public int? IdAnimal { get; set; }

    public int? IdRegistroProduccionLeche { get; set; }

    public DateTime FechaRegistro { get; set; }

    [ForeignKey("IdCentroCosto")]
    [InverseProperty("MovimientoCostos")]
    public virtual CentroCosto IdCentroCostoNavigation { get; set; } = null!;

    [ForeignKey("IdTipoCosto")]
    [InverseProperty("MovimientoCostos")]
    public virtual TipoCosto IdTipoCostoNavigation { get; set; } = null!;
}
