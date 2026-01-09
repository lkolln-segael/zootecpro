using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Enfermedad")]
[Index("idAnimal", Name = "IX_Enfermedad_idAnimal")]
[Index("idTipoEnfermedad", Name = "IX_Enfermedad_idTipoEnfermedad")]
[Index("idVeterinario", Name = "IX_Enfermedad_idVeterinario")]
public partial class Enfermedad
{
    [Key]
    public int Id { get; set; }

    [Precision(0)]
    public DateTime fechaDiagnostico { get; set; }

    [Precision(0)]
    public DateTime? fechaRecuperacion { get; set; }

    public int idVeterinario { get; set; }

    public int idTipoEnfermedad { get; set; }

    public int idAnimal { get; set; }

    [InverseProperty("idEnfermedadNavigation")]
    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    [ForeignKey("idAnimal")]
    [InverseProperty("Enfermedads")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;

    [ForeignKey("idTipoEnfermedad")]
    [InverseProperty("Enfermedads")]
    public virtual TipoEnfermedade idTipoEnfermedadNavigation { get; set; } = null!;

    [ForeignKey("idVeterinario")]
    [InverseProperty("Enfermedads")]
    public virtual Usuario idVeterinarioNavigation { get; set; } = null!;
}
