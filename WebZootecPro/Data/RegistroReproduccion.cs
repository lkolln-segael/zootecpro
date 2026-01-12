using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("RegistroReproduccion")]
[Index("idAnimal", Name = "IX_RegistroReproduccion_idAnimal")]
[Index("idHato", Name = "IX_RegistroReproduccion_idHato")]
public partial class RegistroReproduccion
{
    [Key]
    public int Id { get; set; }

    [StringLength(260)]
    public string? fotoVaca { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    public int idAnimal { get; set; }

    public int? idHato { get; set; }

    [InverseProperty("idRegistroReproduccionNavigation")]
    public virtual ICollection<Aborto> Abortos { get; set; } = new List<Aborto>();

    [InverseProperty("idRegistroReproduccionNavigation")]
    public virtual ICollection<ConfirmacionPrenez> ConfirmacionPrenezs { get; set; } = new List<ConfirmacionPrenez>();

    [InverseProperty("idRegistroReproduccionNavigation")]
    public virtual ICollection<Parto> Partos { get; set; } = new List<Parto>();

    [InverseProperty("idRegistroReproduccionNavigation")]
    public virtual ICollection<Prenez> Prenezs { get; set; } = new List<Prenez>();

    [InverseProperty("idRegistroReproduccionNavigation")]
    public virtual ICollection<RegistroNacimiento> RegistroNacimientos { get; set; } = new List<RegistroNacimiento>();

    [InverseProperty("idRegistroReproduccionNavigation")]
    public virtual ICollection<Seca> Secas { get; set; } = new List<Seca>();

    [ForeignKey("idAnimal")]
    [InverseProperty("RegistroReproduccions")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;

    [ForeignKey("idHato")]
    [InverseProperty("RegistroReproduccions")]
    public virtual Hato? idHatoNavigation { get; set; }
}
