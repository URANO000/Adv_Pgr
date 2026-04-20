using AP_WEB.Models.Adopciones;
using AP_WEB.Models.Mascotas;
using AP_WEB.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AP_WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MascotaController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPasswordHelper _password;

        public MascotaController(IConfiguration config, IPasswordHelper password)
        {
            _config = config;
            _password = password;
        }

        [HttpPost("RegistrarPublicacionMascota")]
        public IActionResult RegistrarPublicacionMascota(RegistrarPublicacionMascotaRequest model)
        {
            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", model.UsuarioId);
                parametros.Add("@TipoId", model.TipoId);
                parametros.Add("@Nombre", model.Nombre);
                parametros.Add("@Peso", model.Peso);
                parametros.Add("@Edad", model.Edad);
                parametros.Add("@Sexo", model.Sexo);
                parametros.Add("@Enfermedades", model.Enfermedades);
                parametros.Add("@HistorialMedico", model.HistorialMedico);
                parametros.Add("@PreferenciasAlimenticias", model.PreferenciasAlimenticias);
                parametros.Add("@Notas", model.Notas);
                parametros.Add("@Titulo", model.Titulo);
                parametros.Add("@Descripcion", model.Descripcion);

                var result = context.QueryFirstOrDefault<RegistrarPublicacionMascotaResult>(
                    "sp_RegistrarPublicacionMascota",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null || result.PublicacionId <= 0 || result.AnimalId <= 0)
                    return BadRequest("La publicación no se registró correctamente");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ListarMisPublicacionesMascota/{usuarioId}")]
        public IActionResult ListarMisPublicacionesMascota(string usuarioId)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", usuarioId);

            var result = context.Query<PublicacionMascotaResponse>("sp_ListarMisPublicacionesMascota", parametros, commandType: CommandType.StoredProcedure);

            if (result == null || !result.Any())
                return NotFound("No se encontraron publicaciones");

            return Ok(result);
        }

        [HttpGet("ObtenerPublicacionMascota/{publicacionId}/{usuarioId}")]
        public IActionResult ObtenerPublicacionMascota(int publicacionId, string usuarioId)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@PublicacionId", publicacionId);
            parametros.Add("@UsuarioId", usuarioId);

            var result = context.QueryFirstOrDefault<PublicacionMascotaResponse>("sp_ObtenerPublicacionMascota", parametros, commandType: CommandType.StoredProcedure);

            if (result == null)
                return NotFound("La publicación no fue encontrada");

            return Ok(result);
        }

        [HttpPut("EditarPublicacionMascota")]
        public IActionResult EditarPublicacionMascota(EditarPublicacionesMascotaRequest model)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@PublicacionId", model.PublicacionId);
            parametros.Add("@UsuarioId", model.UsuarioId);
            parametros.Add("@Titulo", model.Titulo);
            parametros.Add("@Descripcion", model.Descripcion);

            var resultado = context.ExecuteScalar<int>(
                "sp_EditarPublicacionMascota",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            if (resultado == 0)
                return BadRequest("La publicación no se actualizó correctamente.");

            return Ok("La publicación se actualizó correctamente.");
        }

        [HttpPost("InactivarPublicacionMascota/{publicacionId}/{usuarioId}")]
        public IActionResult InactivarPublicacionMascota(int publicacionId, string usuarioId)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@PublicacionId", publicacionId);
            parametros.Add("@UsuarioId", usuarioId);

            var resultado = context.ExecuteScalar<int>(
                "sp_InactivarPublicacionMascota",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            if (resultado == 0)
                return BadRequest("La publicación no se pudo inactivar");

            return Ok("La publicación se inactivó correctamente");
        }

        [HttpPost("RegistrarAnimalMedia")]
        public IActionResult RegistrarAnimalMedia(RegistrarAnimalMediaRequest model)
        {
            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@AnimalId", model.AnimalId);
                parametros.Add("@ArchivoUrl", model.ArchivoUrl);
                parametros.Add("@FileSize", model.FileSize);
                parametros.Add("@CreatedBy", model.CreatedBy);

                var result = context.QueryFirstOrDefault<RegistrarAnimalMediaResult>(
                    "sp_RegistrarAnimalMedia",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null || result.MediaId <= 0)
                    return BadRequest("La imagen no se registró correctamente");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ListarAnimalMedia/{animalId}")]
        public IActionResult ListarAnimalMedia(int animalId)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@AnimalId", animalId);

            var result = context.Query<AnimalMediaResponse>("sp_ListarAnimalMedia", parametros, commandType: CommandType.StoredProcedure);

            if (result == null || !result.Any())
                return NotFound("No se encontraron imágenes");

            return Ok(result);
        }

        [HttpDelete("EliminarAnimalMedia")]
        public IActionResult EliminarAnimalMedia(EliminarAnimalMediaRequest model)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@MediaId", model.MediaId);
            parametros.Add("@UsuarioId", model.UsuarioId);

            var result = context.Execute("sp_EliminarAnimalMedia", parametros, commandType: CommandType.StoredProcedure);

            if (result <= 0)
                return BadRequest("La imagen no se eliminó correctamente");

            return Ok("La imagen se eliminó correctamente");
        }

        [HttpGet("ListarCatalogoMascotas")]
        public IActionResult ListarCatalogoMascotas(string? texto = null, int? tipoId = null, int? categoriaId = null)
        {
            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@Texto", texto);
                parametros.Add("@TipoId", tipoId);

                var result = context.Query<PublicacionMascotaResponse>(
                    "sp_ListarCatalogoMascotas",
                    parametros,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (result == null || !result.Any())
                    return NotFound("No se encontraron mascotas disponibles.");

                // filtro adicional por categoría con datos que ya vienen de BD
                if (categoriaId.HasValue && categoriaId.Value > 0)
                {
                    result = result.Where(x => x.CategoriaId == categoriaId.Value).ToList();
                }

                if (!result.Any())
                    return NotFound("No se encontraron mascotas con los filtros indicados.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ObtenerDetalleCatalogoMascota/{publicacionId}")]
        public IActionResult ObtenerDetalleCatalogoMascota(int publicacionId)
        {
            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@PublicacionId", publicacionId);

                var result = context.QueryFirstOrDefault<PublicacionMascotaResponse>(
                    "sp_ObtenerDetalleCatalogoMascota",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return NotFound("La publicación no fue encontrada.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("RegistrarSolicitudAdopcion")]
        public IActionResult RegistrarSolicitudAdopcion(RegistrarSolicitudAdopcionRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos para la solicitud de adopción.");

            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@PublicacionId", model.PublicacionId);
                parametros.Add("@UsuarioInteresadoId", model.UsuarioInteresadoId);
                parametros.Add("@Mensaje", model.Mensaje);

                var notif = context.QueryFirstOrDefault<NotificacionSolicitudAdopcionResponse>(
                    "sp_RegistrarSolicitudAdopcion",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (notif == null)
                    return BadRequest("No se pudo registrar la solicitud de adopción.");

                if (!string.IsNullOrWhiteSpace(notif.CorreoElectronico))
                {
                    var contenido = ConstruirCorreoSolicitudAdopcion(notif);
                    _password.EnviarCorreo(
                        notif.CorreoElectronico,
                        "Nueva solicitud de adopción en Patitas Social",
                        contenido
                    );
                }

                return Ok("Solicitud de adopción enviada correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ListarHistorialPublicacionesMascota/{usuarioId}")]
        public IActionResult ListarHistorialPublicacionesMascota(string usuarioId, string estado = "todas")
        {
            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);
                parametros.Add("@Estado", estado);

                var result = context.Query<PublicacionMascotaResponse>(
                    "sp_ListarHistorialPublicacionesMascota",
                    parametros,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (result == null || !result.Any())
                    return NotFound("No se encontraron publicaciones para el estado indicado.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ListarCategoriasCatalogo")]
        public IActionResult ListarCategoriasCatalogo()
        {
            try
            {
                using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

                var parametros = new DynamicParameters();
                parametros.Add("@Texto", null);
                parametros.Add("@TipoId", null);

                var result = context.Query<PublicacionMascotaResponse>(
                    "sp_ListarCatalogoMascotas",
                    parametros,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (result == null || !result.Any())
                    return NotFound("No se encontraron categorías.");

                var categorias = result
                    .Where(x => x.CategoriaId > 0)
                    .Select(x => new
                    {
                        x.CategoriaId,
                        x.CategoriaAnimal
                    })
                    .Distinct()
                    .OrderBy(x => x.CategoriaAnimal)
                    .ToList();

                if (!categorias.Any())
                    return NotFound("No se encontraron categorías.");

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private static string ConstruirCorreoSolicitudAdopcion(NotificacionSolicitudAdopcionResponse datos)
        {
            return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
        </head>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e67e22;'>🐾 ¡Tienes una nueva solicitud de adopción!</h2>

                <p>Hola <strong>{datos.PrimerNombre}</strong>,</p>

                <p>
                    <strong>{datos.NombreInteresado}</strong> ha enviado una solicitud para adoptar a
                    <strong>{datos.NombreMascota}</strong>.
                </p>

                <div style='background:#fef9f0;border-left:4px solid #e67e22;padding:15px;margin:20px 0;border-radius:5px;'>
                    <p style='margin:0;'><strong>Publicación:</strong> {datos.Titulo}</p>
                    <p style='margin:8px 0 0;'><strong>Mascota:</strong> {datos.NombreMascota}</p>
                </div>

                <p>Ingresá a <strong>Patitas Social</strong> para revisar tu publicación y dar seguimiento.</p>

                <hr style='border:none;border-top:1px solid #eee;margin:20px 0;' />
                <p style='color:#999;font-size:12px;'>Patitas Social — Conectando corazones con patitas 🐾</p>
            </div>
        </body>
        </html>";
        }
    }
}