namespace AP_WEB.Models.Mascotas
{
    public class AnimalMediaResponse
    {
        public int MediaId { get; set; }
        public int AnimalId { get; set; }
        public string ArchivoUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
