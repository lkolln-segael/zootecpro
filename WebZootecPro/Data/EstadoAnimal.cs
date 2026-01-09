using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("EstadoAnimal")]
[Index("nombre", Name = "UQ__EstadoAn__72AFBCC6DE71D88B", IsUnique = true)]
public partial class EstadoAnimal
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string nombre { get; set; } = null!;

    [InverseProperty("estado")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
