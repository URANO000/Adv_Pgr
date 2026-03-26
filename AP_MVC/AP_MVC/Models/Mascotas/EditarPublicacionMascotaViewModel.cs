using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Mascotas
{
    public class EditarPublicacionMascotaViewModel
    {
        [Required]
        public int PublicacionId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}