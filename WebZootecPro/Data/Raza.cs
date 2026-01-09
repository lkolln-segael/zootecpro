using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Raza")]
[Index("nombre", Name = "UQ__Raza__72AFBCC6C6A94B6B", IsUnique = true)]
public partial class Raza
{
    [Key]
    public int Id { get; set; }

    [StringLength(80)]
    public string nombre { get; set; } = null!;

    [InverseProperty("idRazaNavigation")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
