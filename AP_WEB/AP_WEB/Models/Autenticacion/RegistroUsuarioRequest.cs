using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Autenticacion
{
    public class RegistroUsuarioRequest
    {
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
        [Required]
        public string Cedula { get; set; } = string.Empty;
        [Required]
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; } = string.Empty;
        [Required]
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; } = string.Empty;
        public string? Telefono { get; set; } = string.Empty;
        public string? Provincia { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }
}
