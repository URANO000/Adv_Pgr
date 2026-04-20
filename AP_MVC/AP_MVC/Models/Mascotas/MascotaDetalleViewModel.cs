namespace AP_MVC.Models.Mascotas
{
    public class MascotaDetalleViewModel
    {
        public int PublicacionId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public int AnimalId { get; set; }
        public string NombreUsuarioSesion { get; set; } = string.Empty;
        public string CorreoUsuarioSesion { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreMascota { get; set; } = string.Empty;
        public decimal? Peso { get; set; }
        public string? Edad { get; set; }
        public string? Sexo { get; set; }
        public string? Enfermedades { get; set; }
        public string? HistorialMedico { get; set; }
        public string? PreferenciasAlimenticias { get; set; }
        public string? Notas { get; set; }

        public int TipoId { get; set; }
        public string TipoAnimal { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string CategoriaAnimal { get; set; } = string.Empty;

        public List<AnimalMediaViewModel> Imagenes { get; set; } = new();
    }
}