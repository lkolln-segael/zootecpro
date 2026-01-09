using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Index("Nombre", Name = "UQ_SexoCria_Nombre", IsUnique = true)]
public partial class SexoCrium
{
    [Key]
    public int Id { get; set; }

    [StringLength(60)]
    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public int Orden { get; set; }

    [InverseProperty("idSexoCriaNavigation")]
    public virtual ICollection<Parto> Partos { get; set; } = new List<Parto>();
}
