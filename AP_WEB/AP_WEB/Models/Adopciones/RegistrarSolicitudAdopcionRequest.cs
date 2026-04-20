using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Adopciones
{
    public class RegistrarSolicitudAdopcionRequest
    {
        [Required]
        public int PublicacionId { get; set; }

        [Required]
        public string UsuarioInteresadoId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;
    }
}