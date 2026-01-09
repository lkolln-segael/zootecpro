using System;
using System.Collections.Generic;

namespace WebZootecPro.ViewModels.Animales
{
    public class HistorialAnimalViewModel
    {
        public int AnimalId { get; set; }
        public string? Nombre { get; set; }
        public string? Codigo { get; set; }
        public List<HistorialMovimientoVm> Movimientos { get; set; } = new();
    }

    public class HistorialMovimientoVm
    {
        public DateTime? Fecha { get; set; }
        public int Orden { get; set; }
        public string Fuente { get; set; } = "";
        public string Evento { get; set; } = "";
        public string Detalle { get; set; } = "";
    }
}
