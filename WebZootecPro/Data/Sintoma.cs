using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Index("idTipoEnfermedad", Name = "IX_Sintomas_idTipoEnfermedad")]
public partial class Sintoma
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    public int idTipoEnfermedad { get; set; }

    [ForeignKey("idTipoEnfermedad")]
    [InverseProperty("Sintomas")]
    public virtual TipoEnfermedade idTipoEnfermedadNavigation { get; set; } = null!;
}
