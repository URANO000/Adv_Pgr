namespace AP_MVC.Models.Mascotas
{
    public class AnimalMediaViewModel
    {
        public int MediaId { get; set; }
        public int AnimalId { get; set; }
        public string ArchivoUrl { get; set; } = string.Empty;
        public int? FileSize { get; set; }
        public string? CreatedBy { get; set; }
    }
}