using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Calidad")]
[Index("idRegistroProduccionLeche", Name = "IX_Calidad_idRegistroProduccionLeche")]
public partial class Calidad
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? grasa { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? proteina { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? solidosTotales { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? urea { get; set; }

    public int idRegistroProduccionLeche { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    public int? rcs { get; set; }

    [ForeignKey("idRegistroProduccionLeche")]
    [InverseProperty("Calidads")]
    public virtual RegistroProduccionLeche idRegistroProduccionLecheNavigation { get; set; } = null!;
}
