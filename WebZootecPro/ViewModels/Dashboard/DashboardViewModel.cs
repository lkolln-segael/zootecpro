namespace WebZootecPro.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public DateOnly Desde { get; set; }
        public DateOnly Hasta { get; set; }

        // Producción
        public decimal ProduccionHoyLitros { get; set; }
        public int VacasProduciendoHoy { get; set; }
        public decimal PromedioLitrosPorVacaHoy { get; set; }
        public decimal ProduccionUltimos7DiasLitros { get; set; }
        public decimal PromedioDiario7Dias { get; set; }

        // Reproducción (30 días por defecto)
        public int ServiciosEnPeriodo { get; set; }
        public int ConcepcionesEnPeriodo { get; set; }
        public decimal TasaPrenezPct { get; set; }
        public decimal TasaInseminacionPct { get; set; }

        // Enfermería
        public int CasosEnfermeriaActivos { get; set; }
        public int CasosNuevos7Dias { get; set; }

        // Alimentación RTM (hoy)
        public int EntregasRtmHoy { get; set; }
        public decimal KgRtmHoy { get; set; }

        // Costos (30 días)
        public decimal CostoUltimos30Dias { get; set; }
        public decimal CostoPorLitroAprox { get; set; }
    }
}
