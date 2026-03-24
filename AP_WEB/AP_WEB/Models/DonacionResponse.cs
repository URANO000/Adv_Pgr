namespace AP_WEB.Models
{
    public class DonacionResponse
    {
        public int DonacionId { get; set; }
        public int FundraiserId { get; set; }
        public string? UsuarioId { get; set; }
        public string NombreDonante { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime DonatedAt { get; set; }
    }
}
