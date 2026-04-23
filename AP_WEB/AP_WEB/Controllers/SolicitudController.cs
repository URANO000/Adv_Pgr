using AP_WEB.Models.Adopciones;
using AP_WEB.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AP_WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudAdopcionController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPasswordHelper _password;

        public SolicitudAdopcionController(IConfiguration config, IPasswordHelper password)
        {
            _config = config;
            _password = password;
        }

        // RF-022 (Isaac): Listar solicitudes recibidas por publicación
        [HttpGet("ListarSolicitudesPorPublicacion/{publicacionId}/{propietarioId}")]
        public IActionResult ListarSolicitudesPorPublicacion(int publicacionId, string propietarioId)
        {
            try
            {
                if (string.IsNullOrEmpty(propietarioId))
                    return BadRequest("PropietarioId requerido");

                using var context = new SqlConnection(
                    _config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@PublicacionId", publicacionId);
                parametros.Add("@PropietarioId", propietarioId);

                var result = context.Query<SolicitudAdopcionResponse>(
                    "sp_ListarSolicitudesPorPublicacion",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(result ?? Enumerable.Empty<SolicitudAdopcionResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // RF-022 – Aprobar o rechazar una solicitud
        // RF-026 – Notifica al interesado sobre el cambio de estado
        [HttpPost("GestionarSolicitudAdopcion")]
        public IActionResult GestionarSolicitudAdopcion(GestionarSolicitudAdopcionRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos para gestionar la solicitud.");

            try
            {
                using var context = new SqlConnection(
                    _config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@SolicitudId", model.SolicitudId);
                parametros.Add("@PropietarioId", model.PropietarioId);
                parametros.Add("@NuevoEstado", model.NuevoEstado);

                var notif = context.QueryFirstOrDefault<NotificacionCambioEstadoSolicitudResponse>(
                    "sp_GestionarSolicitudAdopcion",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (notif == null)
                    return BadRequest("No se pudo gestionar la solicitud.");

                // RF-026: Enviar correo al interesado
                if (!string.IsNullOrWhiteSpace(notif.CorreoElectronico))
                {
                    var contenido = ConstruirCorreoCambioEstado(notif);
                    _password.EnviarCorreo(
                        notif.CorreoElectronico,
                        "Actualización de tu solicitud de adopción — Patitas Social",
                        contenido
                    );
                }

                return Ok("Solicitud gestionada correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #region Helpers privados

        private static string ConstruirCorreoCambioEstado(NotificacionCambioEstadoSolicitudResponse datos)
        {
            var (color, icono, mensaje) = datos.NuevoEstado switch
            {
                "Aprobada" => ("#27ae60", "✅",
                    "¡Buenas noticias! El propietario aprobó tu solicitud. " +
                    "Podés ponerte en contacto para coordinar la adopción."),
                _ => ("#7f8c8d", "❌",
                    "En esta ocasión tu solicitud no fue aprobada. " +
                    "Te animamos a seguir explorando mascotas en Patitas Social.")
            };

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
    <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>

        <h2 style='color:{color};'>{icono} Actualización de tu solicitud de adopción</h2>

        <p>Hola <strong>{datos.PrimerNombre}</strong>,</p>

        <p>{mensaje}</p>

        <div style='background:#f9f9f9;border-left:4px solid {color};
                    padding:15px;margin:20px 0;border-radius:5px;'>
            <p style='margin:0;'><strong>Publicación:</strong> {datos.Titulo}</p>
            <p style='margin:8px 0 0;'><strong>Mascota:</strong> {datos.NombreMascota}</p>
            <p style='margin:8px 0 0;'><strong>Estado:</strong> {datos.NuevoEstado}</p>
        </div>

        <p>Ingresá a <strong>Patitas Social</strong> para ver el detalle completo.</p>

        <hr style='border:none;border-top:1px solid #eee;margin:20px 0;' />
        <p style='color:#999;font-size:12px;'>Patitas Social — Conectando corazones con patitas 🐾</p>
    </div>
</body>
</html>";
        }

        #endregion
    }
}