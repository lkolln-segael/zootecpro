namespace WebZootecPro.ViewModels.Admin
{
    public class EmpresaAdminIndexViewModel
    {
        public int Id { get; set; }
        public string NombreEmpresa { get; set; } = null!;
        public string? Ruc { get; set; }
        public string? Ubicacion { get; set; }
        public int? CapacidadMaxima { get; set; }

        public string DuenoUserName { get; set; } = null!;
        public string DuenoNombre { get; set; } = null!;
        public string DuenoRol { get; set; } = null!;

        public string? DniColaborador { get; set; }
        public string? EspecialidadColaborador { get; set; }
    }
}
