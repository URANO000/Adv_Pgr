using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models
{
    public class EditarFundraiserRequest
    {
        [Required]
        public int FundraiserId { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;
    }
}