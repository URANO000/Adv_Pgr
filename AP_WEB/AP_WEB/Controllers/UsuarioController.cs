using AP_WEB.Models.Autenticacion;
using AP_WEB.Models.Usuario;
using AP_WEB.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AP_WEB.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IGeneralHelper _helper;
        public UsuarioController(IConfiguration configuration, IGeneralHelper helper)
        {
            _configuration = configuration;
            _helper = helper;
        }

        [HttpPut("CambiarAcceso")]
        public IActionResult CambiarAcceso(SeguridadRequest model)
        {
            var UsuarioId = User.FindFirst("UsuarioId")?.Value;

            using var context = _helper.CreateConnection();
            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", UsuarioId);
            parametros.Add("@Contrasenna", model.NuevaContrasenna);

            var result = context.Execute("sp_ActualizarContrasenna", parametros);

            if(result <= 0)
            {
                return BadRequest("Su información no se actualizó correctamente");
            }

            return Ok("Su información se actualizó correctamente");
        }

        [HttpGet("VerPerfil")]
        public IActionResult VerPerfil()
        {
            var UsuarioId = User.FindFirst("UsuarioId")?.Value;

            using var context = _helper.CreateConnection();
            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", UsuarioId);

            var model = context.QueryFirstOrDefault<PerfilResponse>("sp_ObtenerUsuario", parametros);
            if(model == null)
            {
                return NotFound("No se encontró el perfil de usuario.");
            }

            return Ok(model);
        }

        [HttpGet("ListaUsuarios")]
        public IActionResult ListaUsuarios()
        {
            using var context = _helper.CreateConnection();

            var model = context.Query<UsuarioResponse>("sp_ObtenerUsuarios");
            if (model == null)
            {
                return NotFound("No se encontraron usuarios.");
            }

            return Ok(model);
        }

        [Authorize]
        [HttpPut("EditarPerfil")]
        public async Task<IActionResult> EditarPerfil(EditarPerfilRequest model)
        {
            var UsuarioId = User.FindFirst("UsuarioId")?.Value;

            if (UsuarioId == null)
                return Unauthorized("Token inválido o sin UsuarioId");

            using var context = _helper.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UsuarioId", UsuarioId);
            parameters.Add("@CorreoElectronico", model.CorreoElectronico);
            parameters.Add("@PrimerNombre", model.PrimerNombre);
            parameters.Add("@SegundoNombre", model.SegundoNombre);
            parameters.Add("@PrimerApellido", model.PrimerApellido);
            parameters.Add("@SegundoApellido", model.SegundoApellido);
            parameters.Add("@Cedula", model.Cedula);
            parameters.Add("@Telefono", model.Telefono);
            parameters.Add("@Provincia", model.Provincia);
            parameters.Add("@ImagenPerfil", model.ImagenPerfil); 

            var result = context.Execute("sp_EditarUsuario", parameters);
            if (result <= 0)
            {
                return BadRequest("El perfil no se actualizó correctamente.");
            }

            return Ok("El perfil fue editado exitosamente.");
        }
    }
}
