using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("EventoGeneral")]
[Index("idAnimal", "fechaEvento", Name = "IX_EventoGeneral_idAnimal_fecha", IsDescending = new[] { false, true })]
public partial class EventoGeneral
{
    [Key]
    public int Id { get; set; }

    public int idAnimal { get; set; }

    [Precision(0)]
    public DateTime fechaEvento { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string tipoEvento { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    public string? tipoAnalisis { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? resultado { get; set; }

    [StringLength(600)]
    public string? descripcion { get; set; }

    public int? idHato { get; set; }

    public int? usuarioId { get; set; }

    [ForeignKey("idAnimal")]
    [InverseProperty("EventoGenerals")]
    public virtual Animal idAnimalNavigation { get; set; } = null!;
}
