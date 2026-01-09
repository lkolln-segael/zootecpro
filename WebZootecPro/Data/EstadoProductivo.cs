using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("EstadoProductivo")]
[Index("nombre", Name = "UQ__EstadoPr__72AFBCC630A86A0B", IsUnique = true)]
public partial class EstadoProductivo
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string nombre { get; set; } = null!;

    [InverseProperty("estadoProductivo")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
