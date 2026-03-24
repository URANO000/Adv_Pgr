using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Autenticacion
{
    public class RecuperarAccesoRequest
    {
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
    }
}
