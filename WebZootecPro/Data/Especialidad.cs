using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Especialidad")]
[Index("Nombre", Name = "UQ_Especialidad_Nombre", IsUnique = true)]
public partial class Especialidad
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("Especialidad")]
    public virtual ICollection<Colaborador> Colaboradors { get; set; } = new List<Colaborador>();
}
