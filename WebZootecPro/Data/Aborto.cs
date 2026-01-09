using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Aborto")]
[Index("idCausaAborto", Name = "IX_Aborto_idCausaAborto")]
[Index("idRegistroReproduccion", Name = "IX_Aborto_idRegistroReproduccion")]
public partial class Aborto
{
    [Key]
    public int Id { get; set; }

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    public int idRegistroReproduccion { get; set; }

    public int idCausaAborto { get; set; }

    public int? diasATermino { get; set; }

    [ForeignKey("idCausaAborto")]
    [InverseProperty("Abortos")]
    public virtual CausaAborto idCausaAbortoNavigation { get; set; } = null!;

    [ForeignKey("idRegistroReproduccion")]
    [InverseProperty("Abortos")]
    public virtual RegistroReproduccion idRegistroReproduccionNavigation { get; set; } = null!;
}
