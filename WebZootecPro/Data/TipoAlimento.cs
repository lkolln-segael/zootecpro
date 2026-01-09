using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("TipoAlimento")]
[Index("idAnimal", Name = "IX_TipoAlimento_idAnimal")]
public partial class TipoAlimento
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? estado { get; set; }

    public int idAnimal { get; set; }

    [InverseProperty("idTipoAlimentoNavigation")]
    public virtual ICollection<Alimentacion> Alimentacions { get; set; } = new List<Alimentacion>();

    [ForeignKey("idAnimal")]
    [InverseProperty("TipoAlimentos")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;
}
