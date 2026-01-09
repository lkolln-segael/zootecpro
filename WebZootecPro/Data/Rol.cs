using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Rol")]
[Index("Nombre", Name = "UQ_Rol_Nombre", IsUnique = true)]
public partial class Rol
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("Rol")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
