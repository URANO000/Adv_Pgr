using AP_MVC.Models.Enum;

namespace AP_MVC.Models
{
    public class Usuario
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasenna { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string SegundoNombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string SegundoApellido { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public Rol RolId { get; set; }
        public string ImagenPerfil { get; set; } = string.Empty;


        public string Token { get; set; } = string.Empty;
    }
}