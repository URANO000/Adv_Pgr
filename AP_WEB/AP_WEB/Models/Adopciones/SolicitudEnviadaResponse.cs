namespace AP_WEB.Models.Adopciones
{
    public class SolicitudEnviadaResponse
    {
        public int SolicitudId { get; set; }
        public int PublicacionId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string NombreMascota { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
        public DateTime SentAt { get; set; }
    }
}