using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Index("idAnimal", Name = "IX_RegistroSalida_idAnimal")]
public partial class RegistroSalidum
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    [StringLength(80)]
    public string tipoSalida { get; set; } = null!;

    public int idAnimal { get; set; }

    public DateOnly fechaSalida { get; set; }

    public int? idHato { get; set; }

    [StringLength(200)]
    public string? destino { get; set; }

    public int? usuarioId { get; set; }

    [StringLength(600)]
    public string? observacion { get; set; }

    [ForeignKey("idAnimal")]
    [InverseProperty("RegistroSalida")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;
}
