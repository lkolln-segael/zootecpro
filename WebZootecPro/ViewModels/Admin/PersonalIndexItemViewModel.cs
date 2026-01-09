namespace WebZootecPro.ViewModels.Admin
{
    public class PersonalIndexItemViewModel
    {
        public int ColaboradorId { get; set; }
        public string Tipo { get; set; } = null!;  
        public string UserName { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Apellido { get; set; }
        public string DNI { get; set; } = null!;
        public string? Telefono { get; set; }
    }
}
