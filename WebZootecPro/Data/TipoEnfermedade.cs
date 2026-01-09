using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Index("nombre", Name = "UQ_TipoEnfermedades_nombre", IsUnique = true)]
public partial class TipoEnfermedade
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string nombre { get; set; } = null!;

    [InverseProperty("idTipoEnfermedadNavigation")]
    public virtual ICollection<Enfermedad> Enfermedads { get; set; } = new List<Enfermedad>();

    [InverseProperty("idTipoEnfermedadNavigation")]
    public virtual ICollection<Sintoma> Sintomas { get; set; } = new List<Sintoma>();

    [InverseProperty("idTipoEnfermedadNavigation")]
    public virtual ICollection<TipoTratamiento> TipoTratamientos { get; set; } = new List<TipoTratamiento>();
}
