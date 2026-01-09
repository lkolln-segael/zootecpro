using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("ProcedenciaAnimal")]
[Index("nombre", Name = "UQ__Proceden__72AFBCC639988B77", IsUnique = true)]
public partial class ProcedenciaAnimal
{
    [Key]
    public int Id { get; set; }

    [StringLength(80)]
    public string nombre { get; set; } = null!;

    [InverseProperty("procedencia")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
