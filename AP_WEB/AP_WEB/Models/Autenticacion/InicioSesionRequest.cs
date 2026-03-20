using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Autenticacion
{
    public class InicioSesionRequest
    {
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
