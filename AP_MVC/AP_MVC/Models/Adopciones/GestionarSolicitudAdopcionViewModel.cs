using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Adopciones
{
    public class GestionarSolicitudAdopcionViewModel
    {
        [Required]
        public int SolicitudId { get; set; }

        [Required]
        public int PublicacionId { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [RegularExpression("Aprobada|Rechazada|Revisar",
            ErrorMessage = "El estado debe ser 'Aprobada', 'Rechazada' o 'Revisar'.")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}