using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Establo")]
[Index("EmpresaId", Name = "IX_Establo_EmpresaId")]
public partial class Establo
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

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

    public int EmpresaId { get; set; }

    public int? pveDias { get; set; }

    [InverseProperty("Establo")]
    public virtual ICollection<CampaniaLechera> CampaniaLecheras { get; set; } = new List<CampaniaLechera>();

    [ForeignKey("EmpresaId")]
    [InverseProperty("Establos")]
    public virtual Empresa Empresa { get; set; } = null!;

    [InverseProperty("Establo")]
    public virtual ICollection<Hato> Hatos { get; set; } = new List<Hato>();

    [InverseProperty("idEstabloNavigation")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
