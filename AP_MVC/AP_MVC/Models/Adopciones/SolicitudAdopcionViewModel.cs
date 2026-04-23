namespace AP_MVC.Models.Adopciones
{
    public class SolicitudAdopcionViewModel
    {
        public int SolicitudId { get; set; }
        public int PublicacionId { get; set; }
        public string NombreInteresado { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
        public DateTime SentAt { get; set; }
    }
}