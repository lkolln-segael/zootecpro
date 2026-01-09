using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("ReporteIndustriaLeche")]
[Index("fecha", Name = "IX_ReporteIndustriaLeche_fecha")]
[Index("fecha", "turno", "idHato", Name = "UX_ReporteIndustriaLeche_FechaTurnoHato", IsUnique = true)]
public partial class ReporteIndustriaLeche
{
    [Key]
    public int Id { get; set; }

    public DateOnly fecha { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string turno { get; set; } = null!;

    public int idHato { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal pesoReportado { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? observacion { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("ReporteIndustriaLeches")]
    public virtual Hato idHatoNavigation { get; set; } = null!;
}
