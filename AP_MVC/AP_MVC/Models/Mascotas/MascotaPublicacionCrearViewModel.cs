using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Mascotas
{
    public class MascotaPublicacionCrearViewModel
    {
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de animal es obligatorio")]
        [Display(Name = "Tipo de animal")]
        public int TipoId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Peso")]
        [Range(0.1, 9999, ErrorMessage = "El peso debe ser mayor a 0")]
        public decimal? Peso { get; set; }

        [Required(ErrorMessage = "La edad es obligatoria")]
        [Display(Name = "Edad")]
        public string Edad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sexo es obligatorio")]
        [Display(Name = "Sexo")]
        public string Sexo { get; set; } = string.Empty;

        [Display(Name = "Enfermedades")]
        public string? Enfermedades { get; set; }

        [Display(Name = "Historial médico")]
        public string? HistorialMedico { get; set; }

        [Display(Name = "Preferencias alimenticias")]
        public string? PreferenciasAlimenticias { get; set; }

        [Display(Name = "Notas")]
        public string? Notas { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
    }
}