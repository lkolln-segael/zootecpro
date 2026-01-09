using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Usuario")]
[Index("idEstablo", Name = "IX_Usuario_idEstablo")]
[Index("idHato", Name = "IX_Usuario_idHato")]
[Index("nombreUsuario", Name = "UQ_Usuario_nombreUsuario", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int Id { get; set; }

    [StringLength(60)]
    public string nombreUsuario { get; set; } = null!;

    [StringLength(120)]
    public string nombre { get; set; } = null!;

    public int? idEstablo { get; set; }

    public int? idHato { get; set; }

    [StringLength(255)]
    public string contrasena { get; set; } = null!;

    public int RolId { get; set; }

    [InverseProperty("idUsuarioNavigation")]
    public virtual ICollection<Colaborador> Colaboradors { get; set; } = new List<Colaborador>();

    [InverseProperty("usuario")]
    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();

    [InverseProperty("idVeterinarioNavigation")]
    public virtual ICollection<Enfermedad> Enfermedads { get; set; } = new List<Enfermedad>();

    [ForeignKey("RolId")]
    [InverseProperty("Usuarios")]
    public virtual Rol Rol { get; set; } = null!;

    [InverseProperty("idUsuarioNavigation")]
    public virtual ICollection<RtmEntrega> RtmEntregas { get; set; } = new List<RtmEntrega>();

    [ForeignKey("idEstablo")]
    [InverseProperty("Usuarios")]
    public virtual Establo? idEstabloNavigation { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("Usuarios")]
    public virtual Hato? idHatoNavigation { get; set; }
}
