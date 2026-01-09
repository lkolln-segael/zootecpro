using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("DesarrolloCrecimiento")]
[Index("idAnimal", Name = "IX_DesarrolloCrecimiento_idAnimal")]
public partial class DesarrolloCrecimiento
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? estado { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? pesoActual { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? tamano { get; set; }

    [StringLength(50)]
    public string? condicionCorporal { get; set; }

    [StringLength(50)]
    public string? unidadesAnimal { get; set; }

    public int idAnimal { get; set; }

    [InverseProperty("idUltimoCrecimientoNavigation")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();

    [ForeignKey("idAnimal")]
    [InverseProperty("DesarrolloCrecimientos")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;
}
