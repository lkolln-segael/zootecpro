using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("IngredienteNutriente")]
public partial class IngredienteNutriente
{
    [Key]
    public int IdIngredienteNutriente { get; set; }

    public int IdRtmIngrediente { get; set; }

    public int IdNutriente { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal ValorPorMS { get; set; }
}
