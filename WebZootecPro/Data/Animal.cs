using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Table("Animal")]
[Index("IdCategoriaAnimal", Name = "IX_Animal_IdCategoriaAnimal")]
[Index("codigo", Name = "IX_Animal_codigo")]
[Index("idHato", Name = "IX_Animal_idHato")]
[Index("idMadre", Name = "IX_Animal_idMadre")]
[Index("idPadre", Name = "IX_Animal_idPadre")]
public partial class Animal
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string nombre { get; set; } = null!;

    [StringLength(20)]
    public string? sexo { get; set; }

    [StringLength(60)]
    public string? codigo { get; set; }

    [StringLength(80)]
    public string? IdentificadorElectronico { get; set; }

    [StringLength(80)]
    public string? OtroIdentificador { get; set; }

    [StringLength(50)]
    public string? color { get; set; }

    public DateOnly? fechaNacimiento { get; set; }

    [StringLength(600)]
    public string? observaciones { get; set; }

    public int idHato { get; set; }

    public int? idPadre { get; set; }

    public int? idMadre { get; set; }

    public int? idUltimoCrecimiento { get; set; }

    public int? estadoId { get; set; }

    public int? propositoId { get; set; }

    public int? idRaza { get; set; }

    public int? procedenciaId { get; set; }

    public bool nacimientoEstimado { get; set; }

    public int? estadoProductivoId { get; set; }

    public int? IdCategoriaAnimal { get; set; }

    [StringLength(30)]
    public string? arete { get; set; }

    [StringLength(20)]
    public string? naab { get; set; }

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<Alimentacion> Alimentacions { get; set; } = new List<Alimentacion>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<DesarrolloCrecimiento> DesarrolloCrecimientos { get; set; } = new List<DesarrolloCrecimiento>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<Enfermedad> Enfermedads { get; set; } = new List<Enfermedad>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<EventoGeneral> EventoGenerals { get; set; } = new List<EventoGeneral>();

    [ForeignKey("IdCategoriaAnimal")]
    [InverseProperty("Animals")]
    public virtual CategoriaAnimal? IdCategoriaAnimalNavigation { get; set; }

    [InverseProperty("idMadreNavigation")]
    public virtual ICollection<Animal> InverseidMadreNavigation { get; set; } = new List<Animal>();

    [InverseProperty("idPadreNavigation")]
    public virtual ICollection<Animal> InverseidPadreNavigation { get; set; } = new List<Animal>();

    [InverseProperty("idMadreAnimalNavigation")]
    public virtual ICollection<Prenez> PrenezidMadreAnimalNavigations { get; set; } = new List<Prenez>();

    [InverseProperty("idPadreAnimalNavigation")]
    public virtual ICollection<Prenez> PrenezidPadreAnimalNavigations { get; set; } = new List<Prenez>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<RegistroIngreso> RegistroIngresos { get; set; } = new List<RegistroIngreso>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<RegistroNacimiento> RegistroNacimientos { get; set; } = new List<RegistroNacimiento>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<RegistroProduccionLeche> RegistroProduccionLeches { get; set; } = new List<RegistroProduccionLeche>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<RegistroReproduccion> RegistroReproduccions { get; set; } = new List<RegistroReproduccion>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<RegistroSalidum> RegistroSalida { get; set; } = new List<RegistroSalidum>();

    [InverseProperty("idAnimalNavigation")]
    public virtual ICollection<TipoAlimento> TipoAlimentos { get; set; } = new List<TipoAlimento>();

    [ForeignKey("estadoId")]
    [InverseProperty("Animals")]
    public virtual EstadoAnimal? estado { get; set; }

    [ForeignKey("estadoProductivoId")]
    [InverseProperty("Animals")]
    public virtual EstadoProductivo? estadoProductivo { get; set; }

    [ForeignKey("idHato")]
    [InverseProperty("Animals")]
    public virtual Hato idHatoNavigation { get; set; } = null!;

    [ForeignKey("idMadre")]
    [InverseProperty("InverseidMadreNavigation")]
    public virtual Animal? idMadreNavigation { get; set; }

    [ForeignKey("idPadre")]
    [InverseProperty("InverseidPadreNavigation")]
    public virtual Animal? idPadreNavigation { get; set; }

    [ForeignKey("idRaza")]
    [InverseProperty("Animals")]
    public virtual Raza? idRazaNavigation { get; set; }

    [ForeignKey("idUltimoCrecimiento")]
    [InverseProperty("Animals")]
    public virtual DesarrolloCrecimiento? idUltimoCrecimientoNavigation { get; set; }

    [ForeignKey("procedenciaId")]
    [InverseProperty("Animals")]
    public virtual ProcedenciaAnimal? procedencia { get; set; }

    [ForeignKey("propositoId")]
    [InverseProperty("Animals")]
    public virtual PropositoAnimal? proposito { get; set; }
}
