namespace AP_WEB.Models.Usuario
{
    public class EditarPerfilRequest
    {
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; } = string.Empty;
        public string? Telefono { get; set; } = string.Empty;
        public string? Provincia { get; set; } = string.Empty;
        public string? ImagenPerfil { get; set; } = string.Empty;
    }
}
