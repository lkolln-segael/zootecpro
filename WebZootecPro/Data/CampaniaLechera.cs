using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("CampaniaLechera")]
[Index("EstabloId", "fechaInicio", "fechaFin", Name = "IX_CampaniaLechera_Establo_Fechas")]
[Index("EstabloId", "nombre", Name = "UX_CampaniaLechera_Establo_Nombre", IsUnique = true)]
public partial class CampaniaLechera
{
    [Key]
    public int Id { get; set; }

    public int EstabloId { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    public DateOnly fechaInicio { get; set; }

    public DateOnly fechaFin { get; set; }

    public bool activa { get; set; }

    [StringLength(400)]
    public string? observaciones { get; set; }

    [ForeignKey("EstabloId")]
    [InverseProperty("CampaniaLecheras")]
    public virtual Establo Establo { get; set; } = null!;
}
