using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("CalidadDiariaHato")]
[Index("idHato", "fecha", Name = "IX_CalidadDiariaHato_Hato_Fecha")]
[Index("fecha", "idHato", "fuente", Name = "UX_CalidadDiariaHato_Fecha_Hato_Fuente", IsUnique = true)]
public partial class CalidadDiariaHato
{
    [Key]
    public int Id { get; set; }

    public DateOnly fecha { get; set; }

    public int idHato { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string fuente { get; set; } = null!;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? grasa { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? proteina { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? solidosTotales { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? urea { get; set; }

    public int? rcs { get; set; }

    [StringLength(400)]
    public string? observaciones { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("CalidadDiariaHatos")]
    public virtual Hato idHatoNavigation { get; set; } = null!;
}
