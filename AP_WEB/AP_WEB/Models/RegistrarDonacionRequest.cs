using System.ComponentModel.DataAnnotations;

namespace AP_WEB.Models
{
    public class RegistrarDonacionRequest
    {
        [Required]
        public int FundraiserId { get; set; }

        // Nullable: permite donaciones anónimas
        public string? UsuarioId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Total { get; set; }

        // "PayPal" o "SINPE" — solo para simulación, no se persiste
        public string MetodoPago { get; set; } = string.Empty;
    }
}