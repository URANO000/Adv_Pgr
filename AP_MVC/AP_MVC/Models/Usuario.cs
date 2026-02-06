using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models
{
    public class Usuario
    {
        [Required(ErrorMessage = "La identificación es obligatoria")]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre Completo")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [Display(Name = "Correo Electrónico")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La provincia es obligatoria")]
        [Display(Name = "Provincia")]
        public string Provincia { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [Display(Name = "Contraseña")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Contrasenna { get; set; } = string.Empty;

        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Contrasenna", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasenna { get; set; } = string.Empty;
    }
}