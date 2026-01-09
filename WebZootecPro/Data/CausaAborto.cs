using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("CausaAborto")]
[Index("Nombre", Name = "UQ_CausaAborto_Nombre", IsUnique = true)]
public partial class CausaAborto
{
    [Key]
    public int Id { get; set; }

    [StringLength(80)]
    public string Nombre { get; set; } = null!;

    public bool Oculto { get; set; }

    [InverseProperty("idCausaAbortoNavigation")]
    public virtual ICollection<Aborto> Abortos { get; set; } = new List<Aborto>();
}
