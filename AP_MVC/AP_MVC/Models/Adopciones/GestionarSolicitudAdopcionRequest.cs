using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Adopciones
{
    public class GestionarSolicitudAdopcionRequest
    {
        [Required]
        public int SolicitudId { get; set; }

        [Required]
        public string PropietarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [RegularExpression("Aprobada|Rechazada|Revisar",
            ErrorMessage = "El estado debe ser 'Aprobada', 'Rechazada' o 'Revisar'.")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}