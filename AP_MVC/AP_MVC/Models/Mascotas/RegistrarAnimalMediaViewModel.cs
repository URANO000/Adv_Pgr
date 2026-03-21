using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Mascotas
{
    public class RegistrarAnimalMediaViewModel
    {
        public int AnimalId { get; set; }

        [Required(ErrorMessage = "La URL de la imagen es obligatoria")]
        [Display(Name = "URL de imagen")]
        public string ArchivoUrl { get; set; } = string.Empty;

        [Display(Name = "Tamaño del archivo")]
        public int? FileSize { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
    }
}