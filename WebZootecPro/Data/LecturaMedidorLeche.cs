using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("LecturaMedidorLeche")]
[Index("CodigoMedidor", "FechaHoraLectura", Name = "IX_LecturaMedidor_CodigoMedidor_Fecha")]
[Index("Procesado", "FechaHoraLectura", Name = "IX_LecturaMedidor_Procesado")]
public partial class LecturaMedidorLeche
{
    [Key]
    public int IdLecturaMedidorLeche { get; set; }

    [StringLength(50)]
    public string CodigoMedidor { get; set; } = null!;

    [StringLength(50)]
    public string? CodigoAnimal { get; set; }

    public int? IdAnimal { get; set; }

    public DateTime FechaHoraLectura { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal PesoLecheKg { get; set; }

    public byte NumeroOrdeno { get; set; }

    public bool Procesado { get; set; }

    public int? IdRegistroProduccionLeche { get; set; }

    [StringLength(200)]
    public string? Observacion { get; set; }
}
