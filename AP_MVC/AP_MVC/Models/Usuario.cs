using AP_MVC.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models
{
    public class Usuario
    {
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato incorrecto")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contraseña es obligatoria.")]
        public string Contrasenna { get; set; } = string.Empty;

        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; } = string.Empty;

        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        public string Cedula { get; set; } = string.Empty;
        public string? Telefono { get; set; } = string.Empty;
        public string? Provincia { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public Rol RolId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string ImagenPerfil { get; set; } = string.Empty;


        public string Token { get; set; } = string.Empty;

        //Para sacar nombre completo
        public string nombreCompleto =>
            string.Join(" ", new[] { PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        //Para recordar
        public bool RememberMe { get; set; }
    }
}