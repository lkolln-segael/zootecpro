using WebZootecPro.Data;

namespace WebZootecPro.ViewModels.Animales
{
    public class AnimalDetallesViewModel
    {
        public Animal Animal { get; set; } = null!;

        // Abuelos
        public Animal? AbueloPaterno { get; set; }
        public Animal? AbuelaPaterna { get; set; }
        public Animal? AbueloMaterno { get; set; }
        public Animal? AbuelaMaterna { get; set; }

        // Producción por campaña
        public List<ProduccionCampaniaItemVm> ProduccionPorCampania { get; set; } = new();
    }

    public class ProduccionCampaniaItemVm
    {
        public int IdCampania { get; set; }
        public string Nombre { get; set; } = "";
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }

        public decimal Producido { get; set; }
        public decimal Industria { get; set; }
        public decimal Terneros { get; set; }
        public decimal Descartada { get; set; }
        public decimal VentaDirecta { get; set; }
    }
}
