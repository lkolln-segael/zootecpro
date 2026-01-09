using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Index("Codigo", Name = "UX_PlanLicencia_Codigo", IsUnique = true)]
public partial class PlanLicencium
{
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Codigo { get; set; } = null!;

    [StringLength(120)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Precio { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string Moneda { get; set; } = null!;

    public bool EsIndefinido { get; set; }

    public int? MaxAnimales { get; set; }

    public int? MaxEstablos { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaRegistro { get; set; }

    [InverseProperty("Plan")]
    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
}
