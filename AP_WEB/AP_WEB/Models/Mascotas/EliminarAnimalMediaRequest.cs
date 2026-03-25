using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models.Mascotas
{
    public class EliminarAnimalMediaRequest
    {
        [Required]
        public int MediaId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
    }
}
