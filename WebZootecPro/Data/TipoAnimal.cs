using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("TipoAnimal")]
[Index("Nombre", Name = "UQ_TipoAnimal_Nombre", IsUnique = true)]
public partial class TipoAnimal
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("idTipoAnimalNavigation")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
