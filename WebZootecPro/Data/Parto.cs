using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Parto")]
[Index("idRegistroReproduccion", Name = "IX_Parto_idRegistroReproduccion")]
public partial class Parto
{
    [Key]
    public int Id { get; set; }

    [StringLength(80)]
    public string tipo { get; set; } = null!;

    [Precision(0)]
    public DateTime fechaRegistro { get; set; }

    public int idRegistroReproduccion { get; set; }

    public int? idSexoCria { get; set; }

    public int? idTipoParto { get; set; }

    public int? idEstadoCria { get; set; }

    [StringLength(60)]
    public string? rpCria1 { get; set; }

    [StringLength(60)]
    public string? rpCria2 { get; set; }

    [StringLength(150)]
    public string? nombreCria1 { get; set; }

    [StringLength(150)]
    public string? nombreCria2 { get; set; }

    [Precision(0)]
    public TimeOnly? horaParto { get; set; }

    [ForeignKey("idEstadoCria")]
    [InverseProperty("Partos")]
    public virtual EstadoCrium? idEstadoCriaNavigation { get; set; }

    [ForeignKey("idRegistroReproduccion")]
    [InverseProperty("Partos")]
    public virtual RegistroReproduccion idRegistroReproduccionNavigation { get; set; } = null!;

    [ForeignKey("idSexoCria")]
    [InverseProperty("Partos")]
    public virtual SexoCrium? idSexoCriaNavigation { get; set; }

    [ForeignKey("idTipoParto")]
    [InverseProperty("Partos")]
    public virtual TipoParto? idTipoPartoNavigation { get; set; }
}
