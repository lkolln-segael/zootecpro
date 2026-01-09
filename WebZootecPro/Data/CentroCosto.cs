using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("CentroCosto")]
public partial class CentroCosto
{
    [Key]
    public int IdCentroCosto { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    [InverseProperty("IdCentroCostoNavigation")]
    public virtual ICollection<MovimientoCosto> MovimientoCostos { get; set; } = new List<MovimientoCosto>();
}
