namespace AP_WEB.Models.Autenticacion
{
    public class UsuarioResponse
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; } = string.Empty;
        public string? Telefono { get; set; } = string.Empty;
        public string? Provincia { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public int RolId { get; set; }
        public string ImagenPerfil { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string nombreCompleto =>
            string.Join(" ", new[] { PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    }
}
