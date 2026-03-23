namespace AP_WEB.Models
{
    public class FundraiserResponse
    {
        public int FundraiserId { get; set; }
        public int AnimalId { get; set; }
        public string NombreAnimal { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal MetaTotal { get; set; }
        public decimal TotalActual { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}