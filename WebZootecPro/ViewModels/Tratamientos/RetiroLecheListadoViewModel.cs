namespace WebZootecPro.ViewModels.Tratamientos
{
    public class RetiroLecheListadoViewModel
    {
        public int Dias { get; set; } = 7;
        public DateTime Hoy { get; set; } = DateTime.Today;
        public List<RetiroLecheItemViewModel> Items { get; set; } = new();
    }

    public class RetiroLecheItemViewModel
    {
        public int IdAnimal { get; set; }
        public string Codigo { get; set; } = "-";
        public string Nombre { get; set; } = "-";

        public DateTime RetiroHasta { get; set; }
        public int DiasRestantes { get; set; }

        // Info del “tratamiento más restrictivo”
        public string TipoTratamiento { get; set; } = "-";
        public DateTime UltimaDosis { get; set; }
        public int RetiroDias { get; set; }
        public DateTime InicioTratamiento { get; set; }
    }
}
