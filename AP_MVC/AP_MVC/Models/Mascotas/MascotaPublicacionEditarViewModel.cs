using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Mascotas
{
    public class MascotaPublicacionEditarViewModel
    {
        public int PublicacionId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es obligatorio")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
    }
}