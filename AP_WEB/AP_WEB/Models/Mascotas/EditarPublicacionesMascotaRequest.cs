using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Mascotas
{
    public class EditarPublicacionesMascotaRequest
    {
        [Required]
        public int PublicacionId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;
    }
}
