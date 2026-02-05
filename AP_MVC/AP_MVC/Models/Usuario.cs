
using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models
{
    public class Usuario
    {
        [Required(ErrorMessage = "La identificación es obligatoria")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
