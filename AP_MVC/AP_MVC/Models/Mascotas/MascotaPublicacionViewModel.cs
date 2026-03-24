namespace AP_MVC.Models.Mascotas
{
    public class MascotaPublicacionViewModel
    {
        public int PublicacionId { get; set; }
        public int AnimalId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public string Edad { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public string ImagenUrl { get; set; } = "/img/no-image.png";
    }
}