using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("CategoriaAnimal")]
public partial class CategoriaAnimal
{
    [Key]
    public int IdCategoriaAnimal { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("IdCategoriaAnimalNavigation")]
    public virtual ICollection<RequerimientoNutricional> RequerimientoNutricionals { get; set; } = new List<RequerimientoNutricional>();
}
