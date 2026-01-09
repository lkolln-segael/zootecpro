namespace WebZootecPro.ViewModels.Animales
{
    public class AnimalesIndexViewModel
    {
        public List<AnimalListadoVm> Animales { get; set; } = new();
        public HistorialAnimalViewModel? Historial { get; set; }
    }
}
