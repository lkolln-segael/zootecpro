using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RequerimientoNutricional")]
public partial class RequerimientoNutricional
{
    [Key]
    public int IdRequerimientoNutricional { get; set; }

    public int IdCategoriaAnimal { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? ProduccionMinLitros { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? ProduccionMaxLitros { get; set; }

    public int IdNutriente { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal? ValorMin { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal? ValorMax { get; set; }

    [ForeignKey("IdCategoriaAnimal")]
    [InverseProperty("RequerimientoNutricionals")]
    public virtual CategoriaAnimal IdCategoriaAnimalNavigation { get; set; } = null!;

    [ForeignKey("IdNutriente")]
    [InverseProperty("RequerimientoNutricionals")]
    public virtual Nutriente IdNutrienteNavigation { get; set; } = null!;
}
