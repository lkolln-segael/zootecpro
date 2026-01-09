using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RegistroNacimiento")]
[Index("idAnimal", Name = "IX_RegistroNacimiento_idAnimal")]
[Index("idRegistroReproduccion", Name = "IX_RegistroNacimiento_idRegistroReproduccion")]
public partial class RegistroNacimiento
{
    [Key]
    public int Id { get; set; }

    [StringLength(600)]
    public string? observacionesNacimiento { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? pesoNacimiento { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? altitud { get; set; }

    [StringLength(200)]
    public string? ubicacion { get; set; }

    public DateOnly fecha { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? temperatura { get; set; }

    public int idAnimal { get; set; }

    public int idRegistroReproduccion { get; set; }

    [ForeignKey("idAnimal")]
    [InverseProperty("RegistroNacimientos")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;

    [ForeignKey("idRegistroReproduccion")]
    [InverseProperty("RegistroNacimientos")]
    public virtual RegistroReproduccion idRegistroReproduccionNavigation { get; set; } = null!;
}
