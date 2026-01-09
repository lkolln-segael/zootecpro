using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RegistroIngreso")]
[Index("idAnimal", Name = "IX_RegistroIngreso_idAnimal")]
[Index("codigoIngreso", Name = "UQ_RegistroIngreso_codigo", IsUnique = true)]
public partial class RegistroIngreso
{
    [Key]
    public int Id { get; set; }

    [StringLength(60)]
    public string codigoIngreso { get; set; } = null!;

    [StringLength(80)]
    public string tipoIngreso { get; set; } = null!;

    public int idAnimal { get; set; }

    public DateOnly fechaIngreso { get; set; }

    public int? idHato { get; set; }

    [StringLength(200)]
    public string? origen { get; set; }

    public int? usuarioId { get; set; }

    [StringLength(600)]
    public string? observacion { get; set; }

    [ForeignKey("idAnimal")]
    [InverseProperty("RegistroIngresos")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;
}
