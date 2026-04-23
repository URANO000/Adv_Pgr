namespace AP_WEB.Models.Adopciones
{
    public class NotificacionCambioEstadoSolicitudResponse
    {
        public string CorreoElectronico { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string NombreMascota { get; set; } = string.Empty;
        public string NuevoEstado { get; set; } = string.Empty;
    }
}