using Dapper;
using AP_WEB.Models;
using AP_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AP_WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundraiserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPasswordHelper _password;

        public FundraiserController(IConfiguration config, IPasswordHelper password)
        {
            _config = config;
            _password = password;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        // RF-017: Obtener fundraiser por ID
        [HttpGet("ObtenerFundraiser/{id}")]
        public IActionResult ObtenerFundraiser(int id)
        {
            try
            {
                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@FundraiserId", id);

                var result = db.QueryFirstOrDefault<FundraiserResponse>(
                    "sp_ObtenerFundraiser",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return NotFound("La publicación no fue encontrada");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // RF-017: Listar fundraisers activos (catálogo público)
        [HttpGet("ListarFundraisers")]
        public IActionResult ListarFundraisers()
        {
            try
            {
                using var db = GetConnection();

                var result = db.Query<FundraiserResponse>(
                    "sp_ListarFundraisers",
                    commandType: CommandType.StoredProcedure
                );

                return Ok(result ?? Enumerable.Empty<FundraiserResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // RF-017: Donaciones de un fundraiser (para el detalle)
        [HttpGet("ListarDonaciones/{fundraiserId}")]
        public IActionResult ListarDonaciones(int fundraiserId)
        {
            try
            {
                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@FundraiserId", fundraiserId);

                var result = db.Query<DonacionResponse>(
                    "sp_ListarDonacionesPorFundraiser",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(result ?? Enumerable.Empty<DonacionResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // RF-022: Historial del usuario (activos + inactivos)
        [HttpGet("ListarHistorial/{usuarioId}")]
        public IActionResult ListarHistorial(string usuarioId)
        {
            try
            {
                if (string.IsNullOrEmpty(usuarioId))
                    return BadRequest("UsuarioId requerido");

                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                var result = db.Query<FundraiserResponse>(
                    "sp_ListarHistorialFundraisers",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(result ?? Enumerable.Empty<FundraiserResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // RF-020: Registrar donación + RF-030: Notificación al dueño
        [HttpPost("RealizarDonacion")]
        public IActionResult RealizarDonacion([FromBody] RegistrarDonacionRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos de donación inválidos");

            try
            {
                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@FundraiserId", model.FundraiserId);
                parametros.Add("@UsuarioId", model.UsuarioId);
                parametros.Add("@Total", model.Total);

                // SP inserta la donación, actualiza TotalActual y retorna datos para el correo
                var notif = db.QueryFirstOrDefault<NotificacionDonacionDto>(
                    "sp_RegistrarDonacion",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                // RF-030: Notificación al dueño del fundraiser
                if (notif != null && !string.IsNullOrEmpty(notif.CorreoElectronico))
                {
                    var contenido = ConstruirCorreoDonacion(notif, model.Total);
                    _password.EnviarCorreo(notif.CorreoElectronico, "¡Recibiste una donación en Patitas Social!", contenido);
                }

                return Ok("¡Donación registrada exitosamente!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Registrar fundraiser — Evelyn
        [HttpPost("RegistrarFundraiser")]
        public IActionResult RegistrarFundraiser([FromBody] RegistrarFundraiserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            try
            {
                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@AnimalId", model.AnimalId);
                parametros.Add("@Titulo", model.Titulo);
                parametros.Add("@Descripcion", model.Descripcion);
                parametros.Add("@MetaTotal", model.MetaTotal);

                var result = db.Execute(
                    "sp_RegistrarFundraiser",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result <= 0)
                    return BadRequest("La publicación no se registró correctamente");

                return Ok("La publicación se registró correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Editar fundraiser — Evelyn
        [HttpPut("EditarFundraiser")]
        public IActionResult EditarFundraiser([FromBody] EditarFundraiserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            try
            {
                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@FundraiserId", model.FundraiserId);
                parametros.Add("@AnimalId", model.AnimalId);
                parametros.Add("@Titulo", model.Titulo);
                parametros.Add("@Descripcion", model.Descripcion);
                parametros.Add("@MetaTotal", model.MetaTotal);

                var result = db.Execute(
                    "sp_EditarFundraiser",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result <= 0)
                    return BadRequest("La publicación no se actualizó correctamente");

                return Ok("La publicación se actualizó correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Inactivar — Evelyn
        [HttpPost("InactivarFundraiser/{id}")]
        public IActionResult InactivarFundraiser(int id)
        {
            try
            {
                using var db = GetConnection();

                var parametros = new DynamicParameters();
                parametros.Add("@FundraiserId", id);

                var result = db.Execute(
                    "sp_InactivarFundraiser",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result <= 0)
                    return BadRequest("La publicación no se pudo inactivar o ya estaba inactiva");

                return Ok("La publicación se inactivó correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DTO interno para RF-030
        private class NotificacionDonacionDto
        {
            public string CorreoElectronico { get; set; } = string.Empty;
            public string PrimerNombre { get; set; } = string.Empty;
            public string TituloFundraiser { get; set; } = string.Empty;
            public decimal NuevoTotal { get; set; }
            public decimal MetaTotal { get; set; }
        }

        private static string ConstruirCorreoDonacion(NotificacionDonacionDto datos, decimal montoDonado)
        {
            var porcentaje = datos.MetaTotal > 0
                ? Math.Round((datos.NuevoTotal / datos.MetaTotal) * 100, 1)
                : 0;

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head><meta charset='UTF-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
  <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
    <h2 style='color:#e67e22;'>🐾 ¡Recibiste una donación!</h2>
    <p>Hola <strong>{datos.PrimerNombre}</strong>,</p>
    <p>Tu campaña <strong>""{datos.TituloFundraiser}""</strong> acaba de recibir una nueva donación.</p>
    <div style='background:#fef9f0;border-left:4px solid #e67e22;padding:15px;margin:20px 0;border-radius:5px;'>
      <p style='margin:0;font-size:18px;'>💰 Monto donado: <strong>₡{montoDonado:N2}</strong></p>
      <p style='margin:8px 0 0;'>Total acumulado: <strong>₡{datos.NuevoTotal:N2}</strong> de ₡{datos.MetaTotal:N2} ({porcentaje}%)</p>
    </div>
    <p>Ingresá a <strong>Patitas Social</strong> para ver el detalle de tu campaña.</p>
    <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
    <p style='color:#999;font-size:12px;'>Patitas Social — Conectando corazones con patitas 🐾</p>
  </div>
</body>
</html>";
        }
    }
}