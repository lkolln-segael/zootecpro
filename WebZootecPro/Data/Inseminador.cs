using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebZootecPro.Data;

[Table("Inseminador")]
public partial class Inseminador
{
  [Key]
  public int Id { get; set; }

  [Required, StringLength(150)]
  public string nombre { get; set; } = null!;

  [Required, StringLength(150)]
  public string apellido { get; set; } = null!;

  [Required]
  public int EmpresaId { get; set; }

  [ForeignKey(nameof(EmpresaId))]
  public virtual Empresa Empresa { get; set; } = null!;
}

