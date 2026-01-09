using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("PropositoAnimal")]
[Index("nombre", Name = "UQ__Proposit__72AFBCC6C403BB3A", IsUnique = true)]
public partial class PropositoAnimal
{
    [Key]
    public int Id { get; set; }

    [StringLength(80)]
    public string nombre { get; set; } = null!;

    [InverseProperty("proposito")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
