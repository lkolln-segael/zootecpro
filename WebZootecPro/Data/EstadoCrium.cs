using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Index("Nombre", Name = "UQ_EstadoCria_Nombre", IsUnique = true)]
public partial class EstadoCrium
{
    [Key]
    public int Id { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public int Orden { get; set; }

    [InverseProperty("idEstadoCriaNavigation")]
    public virtual ICollection<Parto> Partos { get; set; } = new List<Parto>();
}
