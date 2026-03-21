using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Mascotas
{
    public class RegistrarPublicacionMascotaRequest
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        public int TipoId { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public decimal? Peso { get; set; }

        public string? Edad { get; set; }

        public string? Sexo { get; set; }

        public string? Enfermedades { get; set; }

        public string? HistorialMedico { get; set; }

        public string? PreferenciasAlimenticias { get; set; }

        public string? Notas { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;
    }
}
