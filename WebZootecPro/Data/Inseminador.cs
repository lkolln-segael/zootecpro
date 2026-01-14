using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Inseminador")]
public partial class Inseminador
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    [StringLength(150)]
    public string apellido { get; set; } = null!;

    public int EmpresaId { get; set; }

    [ForeignKey("EmpresaId")]
    [InverseProperty("Inseminadors")]
    public virtual Empresa Empresa { get; set; } = null!;

    [InverseProperty("IdInseminadorNavigation")]
    public virtual ICollection<Prenez> Prenezs { get; set; } = new List<Prenez>();
}
