using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Empresa")]
[Index("ruc", Name = "UQ_Empresa_Ruc", IsUnique = true)]
public partial class Empresa
{
    [Key]
    public int Id { get; set; }

    public int usuarioID { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string ruc { get; set; } = null!;

    [StringLength(260)]
    public string? logo { get; set; }

    public int? capacidadMaxima { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? areaTotal { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? areaPasto { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? areaBosque { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? areaCultivos { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? areaConstruida { get; set; }

    [StringLength(200)]
    public string? ubicacion { get; set; }

    [StringLength(150)]
    public string NombreEmpresa { get; set; } = null!;

    public int? PlanId { get; set; }

    [InverseProperty("Empresa")]
    public virtual ICollection<Colaborador> Colaboradors { get; set; } = new List<Colaborador>();

    [InverseProperty("Empresa")]
    public virtual ICollection<Establo> Establos { get; set; } = new List<Establo>();

    [InverseProperty("Empresa")]
    public virtual ICollection<Inseminador> Inseminadors { get; set; } = new List<Inseminador>();

    [ForeignKey("PlanId")]
    [InverseProperty("Empresas")]
    public virtual PlanLicencium? Plan { get; set; }

    [ForeignKey("usuarioID")]
    [InverseProperty("Empresas")]
    public virtual Usuario usuario { get; set; } = null!;
}
