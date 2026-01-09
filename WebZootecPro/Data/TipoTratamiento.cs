using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("TipoTratamiento")]
[Index("idTipoEnfermedad", Name = "IX_TipoTratamiento_idTipoEnfermedad")]
public partial class TipoTratamiento
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? costo { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? cantidad { get; set; }

    [StringLength(50)]
    public string? unidad { get; set; }

    public int idTipoEnfermedad { get; set; }

    public int? retiroLecheDias { get; set; }

    [InverseProperty("idTipoTratamientoNavigation")]
    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    [ForeignKey("idTipoEnfermedad")]
    [InverseProperty("TipoTratamientos")]
    public virtual TipoEnfermedade idTipoEnfermedadNavigation { get; set; } = null!;
}
