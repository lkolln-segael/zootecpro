using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Tratamiento")]
[Index("idEnfermedad", Name = "IX_Tratamiento_idEnfermedad")]
[Index("idTipoTratamiento", Name = "IX_Tratamiento_idTipoTratamiento")]
public partial class Tratamiento
{
    [Key]
    public int Id { get; set; }

    [Precision(0)]
    public DateTime fechaInicio { get; set; }

    [Precision(0)]
    public DateTime? fechaFinalEstimada { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? costoEstimado { get; set; }

    [StringLength(600)]
    public string? observaciones { get; set; }

    public int idTipoTratamiento { get; set; }

    public int idEnfermedad { get; set; }

    [ForeignKey("idEnfermedad")]
    [InverseProperty("Tratamientos")]
    public virtual Enfermedad idEnfermedadNavigation { get; set; } = null!;

    [ForeignKey("idTipoTratamiento")]
    [InverseProperty("Tratamientos")]
    public virtual TipoTratamiento idTipoTratamientoNavigation { get; set; } = null!;
}
