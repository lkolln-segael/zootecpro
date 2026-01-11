using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RegistroProduccionLeche")]
[Index("idAnimal", Name = "IX_RegistroProduccionLeche_idAnimal")]
public partial class RegistroProduccionLeche
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? pesoOrdeno { get; set; }

    [Precision(0)]
    public DateTime? fechaPreparacion { get; set; }

    [Precision(0)]
    public DateTime? fechaLimpieza { get; set; }

    [Precision(0)]
    public DateTime? fechaDespunte { get; set; }

    [Precision(0)]
    public DateTime? fechaColocacionPezoneras { get; set; }

    [Precision(0)]
    public DateTime? fechaOrdeno { get; set; }

    [Precision(0)]
    public DateTime? fechaRetirada { get; set; }

    public int idAnimal { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string turno { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? cantidadIndustria { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? cantidadTerneros { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? cantidadDescartada { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? cantidadVentaDirecta { get; set; }

    public bool tieneAntibiotico { get; set; }

    [StringLength(200)]
    public string? motivoDescarte { get; set; }

    public int? diasEnLeche { get; set; }

    [StringLength(50)]
    public string? fuente { get; set; }

    [InverseProperty("idRegistroProduccionLecheNavigation")]
    public virtual ICollection<Calidad> Calidads { get; set; } = new List<Calidad>();

    [ForeignKey("idAnimal")]
    [InverseProperty("RegistroProduccionLeches")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;
}
