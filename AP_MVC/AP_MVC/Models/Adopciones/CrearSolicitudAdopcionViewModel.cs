using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Adopciones
{
    public class CrearSolicitudAdopcionViewModel
    {
        [Required]
        public int PublicacionId { get; set; }

        public string UsuarioInteresadoId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes escribir un mensaje.")]
        [StringLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;
    }
}