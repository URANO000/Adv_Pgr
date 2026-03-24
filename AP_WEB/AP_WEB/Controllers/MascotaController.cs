using AP_WEB.Models.Mascotas;
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

        public MascotaController(IConfiguration config)
        {
            _config = config;
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

            var result = context.Execute("sp_InactivarPublicacionMascota", parametros, commandType: CommandType.StoredProcedure);

            if (result <= 0)
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
    }
}
