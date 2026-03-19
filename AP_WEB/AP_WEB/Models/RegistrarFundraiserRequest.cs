using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models
{
    public class RegistrarFundraiserRequest
    {
        [Required]
        public int AnimalId { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue)]
        public decimal MetaTotal { get; set; }
    }
}