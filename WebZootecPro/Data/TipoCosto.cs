using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("TipoCosto")]
public partial class TipoCosto
{
    [Key]
    public int IdTipoCosto { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    public bool EsVariable { get; set; }

    [InverseProperty("IdTipoCostoNavigation")]
    public virtual ICollection<MovimientoCosto> MovimientoCostos { get; set; } = new List<MovimientoCosto>();
}
