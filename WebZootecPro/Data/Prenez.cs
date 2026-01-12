using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Prenez")]
[Index("idHato", Name = "IX_Prenez_idHato")]
[Index("idMadreAnimal", Name = "IX_Prenez_idMadreAnimal")]
[Index("idPadreAnimal", Name = "IX_Prenez_idPadreAnimal")]
[Index("idRegistroReproduccion", Name = "IX_Prenez_idRegistroReproduccion")]
public partial class Prenez
{
    [Key]
    public int Id { get; set; }

    public DateOnly? fechaCelo { get; set; }

    public DateOnly? fechaInseminacion { get; set; }

    public DateOnly? fechaDiagnostico { get; set; }

    public int? idPadreAnimal { get; set; }

    public int? idMadreAnimal { get; set; }

    public int idRegistroReproduccion { get; set; }

    [StringLength(600)]
    public string? observacion { get; set; }

    [Precision(0)]
    public TimeOnly? horaServicio { get; set; }

    public int? numeroServicio { get; set; }

    [StringLength(150)]
    public string? nombreToro { get; set; }

    [StringLength(20)]
    public string? codigoNaab { get; set; }

    [StringLength(50)]
    public string? protocolo { get; set; }

    public DateOnly? fechaProbableParto { get; set; }

    public DateOnly? fechaProbableSeca { get; set; }

    public int? idHato { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("Prenezs")]
    public virtual Hato? idHatoNavigation { get; set; }

    [ForeignKey("idMadreAnimal")]
    [InverseProperty("PrenezidMadreAnimalNavigations")]
    public virtual Animal? idMadreAnimalNavigation { get; set; }

    [ForeignKey("idPadreAnimal")]
    [InverseProperty("PrenezidPadreAnimalNavigations")]
    public virtual Animal? idPadreAnimalNavigation { get; set; }

    [ForeignKey("idRegistroReproduccion")]
    [InverseProperty("Prenezs")]
    public virtual RegistroReproduccion idRegistroReproduccionNavigation { get; set; } = null!;
}
