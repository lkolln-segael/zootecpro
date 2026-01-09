using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Nutriente")]
public partial class Nutriente
{
    [Key]
    public int IdNutriente { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(20)]
    public string Unidad { get; set; } = null!;

    [InverseProperty("IdNutrienteNavigation")]
    public virtual ICollection<RequerimientoNutricional> RequerimientoNutricionals { get; set; } = new List<RequerimientoNutricional>();
}
