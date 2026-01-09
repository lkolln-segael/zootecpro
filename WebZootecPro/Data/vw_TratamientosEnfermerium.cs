using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

[Keyless]
public partial class vw_TratamientosEnfermerium
{
    public int IdTratamiento { get; set; }

    public int IdAnimal { get; set; }

    public int idEnfermedad { get; set; }

    public int idTipoTratamiento { get; set; }

    [Precision(0)]
    public DateTime fechaInicio { get; set; }

    [Precision(0)]
    public DateTime? fechaFinalEstimada { get; set; }

    public int? DiasEnEnfermeria { get; set; }

    public int EstaEnEnfermeria { get; set; }
}
