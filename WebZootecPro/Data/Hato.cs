using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Hato")]
[Index("EstabloId", Name = "IX_Hato_EstabloId")]
public partial class Hato
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    [StringLength(150)]
    public string? sistemaProduccion { get; set; }

    [StringLength(200)]
    public string? ubicacion { get; set; }

    public int EstabloId { get; set; }

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<Aborto> Abortos { get; set; } = new List<Aborto>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<CalidadDiariaHato> CalidadDiariaHatos { get; set; } = new List<CalidadDiariaHato>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<ConfirmacionPrenez> ConfirmacionPrenezs { get; set; } = new List<ConfirmacionPrenez>();

    [ForeignKey("EstabloId")]
    [InverseProperty("Hatos")]
    public virtual Establo Establo { get; set; } = null!;

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<Parto> Partos { get; set; } = new List<Parto>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<Prenez> Prenezs { get; set; } = new List<Prenez>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<RegistroReproduccion> RegistroReproduccions { get; set; } = new List<RegistroReproduccion>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<ReporteIndustriaLeche> ReporteIndustriaLeches { get; set; } = new List<ReporteIndustriaLeche>();

    [InverseProperty("hato")]
    public virtual ICollection<RtmEntrega> RtmEntregas { get; set; } = new List<RtmEntrega>();

    [InverseProperty("hato")]
    public virtual ICollection<RtmRacionCorral> RtmRacionCorrals { get; set; } = new List<RtmRacionCorral>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<Seca> Secas { get; set; } = new List<Seca>();

    [InverseProperty("idHatoNavigation")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
