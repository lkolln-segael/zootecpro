using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Colaborador")]
[Index("EmpresaId", Name = "IX_Colaborador_EmpresaId")]
[Index("DNI", Name = "UQ_Colaborador_DNI", IsUnique = true)]
public partial class Colaborador
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string nombre { get; set; } = null!;

    [StringLength(8)]
    [Unicode(false)]
    public string DNI { get; set; } = null!;

    public int EspecialidadId { get; set; }

    public int idUsuario { get; set; }

    public int? EmpresaId { get; set; }

    [StringLength(120)]
    public string? Apellido { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }

    [StringLength(100)]
    public string? Provincia { get; set; }

    [StringLength(100)]
    public string? Localidad { get; set; }

    [StringLength(10)]
    public string? CodigoPostal { get; set; }

    [StringLength(30)]
    public string? Telefono { get; set; }

    [ForeignKey("EmpresaId")]
    [InverseProperty("Colaboradors")]
    public virtual Empresa? Empresa { get; set; }

    [ForeignKey("EspecialidadId")]
    [InverseProperty("Colaboradors")]
    public virtual Especialidad Especialidad { get; set; } = null!;

    [ForeignKey("idUsuario")]
    [InverseProperty("Colaboradors")]
    public virtual Usuario idUsuarioNavigation { get; set; } = null!;
}
