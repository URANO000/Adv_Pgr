using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Autenticacion
{
    public class SeguridadRequest
    {
        public string NuevaContrasenna { get; set; } = string.Empty;
        public string ConfirmarContrasenna { get; set; } = string.Empty;
    }
}
