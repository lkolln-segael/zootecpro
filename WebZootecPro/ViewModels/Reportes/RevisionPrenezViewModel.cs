namespace WebZootecPro.ViewModels.Reportes
{
    public class RevisionPrenezViewModel
    {
        public string Metodo { get; set; } = "ECOGRAFIA"; // ECOGRAFIA / PALPACION
        public int MinDias { get; set; }

        public DateOnly Desde { get; set; }
        public DateOnly Hasta { get; set; }
        public int? IdHato { get; set; }

        public List<RevisionPrenezRowVm> Items { get; set; } = new();
    }

    public class RevisionPrenezRowVm
    {
        public int AnimalId { get; set; }
        public string Codigo { get; set; } = "-";
        public string Nombre { get; set; } = "-";
        public string Hato { get; set; } = "-";

        public DateOnly FechaInseminacion { get; set; }
        public DateOnly FechaRevision { get; set; }
        public int DiasDesdeInseminacion { get; set; }

        public int? NumeroServicio { get; set; }
        public string? NombreToro { get; set; }
        public string? CodigoNaab { get; set; }
        public string? Protocolo { get; set; }
    }
}
