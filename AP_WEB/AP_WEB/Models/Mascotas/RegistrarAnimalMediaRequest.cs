using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Mascotas
{
    public class RegistrarAnimalMediaRequest
    {
        [Required]
        public int AnimalId { get; set; }

        [Required]
        public string ArchivoUrl { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
