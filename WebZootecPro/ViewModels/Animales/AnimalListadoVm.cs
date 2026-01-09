namespace WebZootecPro.ViewModels.Animales
{
    public class AnimalListadoVm
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Sexo { get; set; }

        public DateOnly? FechaNacimiento { get; set; }
        public string EdadTexto { get; set; } = "-";

        public string Hato { get; set; } = "-";

        public string Padre { get; set; } = "-";
        public string Madre { get; set; } = "-";

        public string AbueloPaterno { get; set; } = "-";
        public string AbueloMaterno { get; set; } = "-";

        public int NumeroPartos { get; set; }
    }
}
