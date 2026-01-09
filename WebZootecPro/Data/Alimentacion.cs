using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Alimentacion")]
[Index("idAnimal", Name = "IX_Alimentacion_idAnimal")]
[Index("idTipoAlimento", Name = "IX_Alimentacion_idTipoAlimento")]
public partial class Alimentacion
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? estado { get; set; }

    public int idAnimal { get; set; }

    public int idTipoAlimento { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? cantidad { get; set; }

    public DateOnly fecha { get; set; }

    [ForeignKey("idAnimal")]
    [InverseProperty("Alimentacions")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;

    [ForeignKey("idTipoAlimento")]
    [InverseProperty("Alimentacions")]
    public virtual TipoAlimento idTipoAlimentoNavigation { get; set; } = null!;
}
