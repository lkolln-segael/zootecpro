using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("TipoParto")]
[Index("Nombre", Name = "UQ_TipoParto_Nombre", IsUnique = true)]
public partial class TipoParto
{
    [Key]
    public int Id { get; set; }

    [StringLength(60)]
    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public int Orden { get; set; }

    [InverseProperty("idTipoPartoNavigation")]
    public virtual ICollection<Parto> Partos { get; set; } = new List<Parto>();
}
