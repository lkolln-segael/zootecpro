namespace WebZootecPro.ViewModels.Reportes
{
    public class ReproduccionKpiViewModel
    {
        public DateOnly Desde { get; set; }
        public DateOnly Hasta { get; set; }
        public int? IdHato { get; set; }

        public int TotalHembrasActivas { get; set; }
        public int VacasElegiblesAlCorte { get; set; }

        public int ServiciosEnPeriodo { get; set; }
        public int ConcepcionesEnPeriodo { get; set; }

        public decimal TasaPrenez { get; set; }            // PR = Concepciones / Elegibles * 100
        public decimal TasaInseminacion { get; set; }      // IA = Servicios / Elegibles * 100

        public int TotalPrimerServicio { get; set; }
        public int ConcepcionPrimerServicio { get; set; }
        public decimal ConcepcionPrimerServicioPct { get; set; }

        public decimal ServiciosPorConcepcion { get; set; }

        public double? DiasPrimerServicioProm { get; set; }
        public double? DiasAbiertosProm { get; set; }
        public double? IntervaloEntrePartosProm { get; set; }

        public int Base150DIM { get; set; }
        public int Prenadas150DIM { get; set; }
        public decimal PorcPrenadas150DIM { get; set; }

        public int Base200DIM { get; set; }
        public int Prenadas200DIM { get; set; }
        public decimal PorcPrenadas200DIM { get; set; }

        public int AbortosEnPeriodo { get; set; }
        public int ConfirmPositivasEnPeriodo { get; set; }
        public decimal TasaAbortoPct { get; set; }
    }
}
