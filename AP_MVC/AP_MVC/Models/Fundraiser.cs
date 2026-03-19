using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models
{
    public class Fundraiser
    {
        public int FundraiserId { get; set; }

        [Required(ErrorMessage = "El animal es obligatorio")]
        [Display(Name = "Animal")]
        public int AnimalId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [Display(Name = "Título")]
        [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La meta de recaudación es obligatoria")]
        [Display(Name = "Meta de Recaudación")]
        [Range(1, double.MaxValue, ErrorMessage = "La meta debe ser mayor a 0")]
        public decimal MetaTotal { get; set; }

        [Display(Name = "Publicación Activa")]
        public bool IsActive { get; set; } = true;
    }
}