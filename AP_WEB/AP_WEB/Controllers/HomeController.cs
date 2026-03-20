using AP_WEB.Models.Autenticacion;
using AP_WEB.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AP_WEB.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IGeneralHelper _helper;
        private readonly IPasswordHelper _password;
        public HomeController(IConfiguration config, IGeneralHelper helper, IPasswordHelper password)
        {
            _config = config;
            _helper = helper;
            _password = password;
        }

        [HttpPost("RegistroUsuario")]
        public IActionResult RegistrarUsuario(RegistroUsuarioRequest model)
        {
            using var context = _helper.CreateConnection();
            var parametros = new DynamicParameters();
            parametros.Add("@CorreoElectronico", model.CorreoElectronico);
            parametros.Add("@Contrasenna", model.Contrasenna);
            parametros.Add("@PrimerNombre", model.PrimerNombre);
            parametros.Add("@SegundoNombre", model.SegundoNombre);
            parametros.Add("@PrimerApellido", model.PrimerApellido);
            parametros.Add("@SegundoApellido", model.SegundoApellido);
            parametros.Add("@Cedula", model.Cedula);
            parametros.Add("@Telefono", model.Telefono);
            parametros.Add("@Provincia", model.Provincia);
            parametros.Add("@CreatedAt", model.CreatedAt);

            var result = context.Execute("sp_RegistrarCuenta", parametros);
            if(result <= 0)
            {
                return BadRequest("Su información no se registró correctamente");
            }

            return Ok("Su información se registró correctamente");
        }

        [HttpPost("IniciarSesion")]
        public IActionResult IniciarSesion(InicioSesionRequest model)
        {
            using var context = _helper.CreateConnection();
            var parametros = new DynamicParameters();
            parametros.Add("@CorreoElectronico", model.CorreoElectronico);
            parametros.Add("@Contrasenna", model.Contrasenna);

            var result = context.QueryFirstOrDefault<UsuarioResponse>("sp_IniciarSesion", parametros);

            if (result == null)
            {
                return NotFound("Su información no se autenticó correctamente");
            }

            result.Token = GenerarToken(result.UsuarioId);

            return Ok(result);
        }

        [HttpPut("RecuperarAcceso")]
        public IActionResult RecuperarAcceso(RecuperarAccesoRequest model)
        {
            using var context = _helper.CreateConnection();

            //Validamos el correo
            var parametros = new DynamicParameters();
            parametros.Add("@CorreoElectronico", model.CorreoElectronico);
            var result = context.QueryFirstOrDefault<UsuarioResponse>("sp_ValidarCorreo", parametros);

            if(result == null)
            {
                return NotFound("Su información no se validó correctamente");
            }

            //Se genera una nueva contraseña
            var nuevaContrasenna = GenerarContrasenna();

            //Actualizamos la contraseña
            var parametrosActualizacion = new DynamicParameters();
            parametrosActualizacion.Add("@UsuarioId", result.UsuarioId);
            parametrosActualizacion.Add("@Contrasenna", _password.Encrypt(nuevaContrasenna));
            var actualizacion = context.Execute("sp_ActualizarContrasenna", parametrosActualizacion);

            //Notif con correo
            var nombre = result.PrimerNombre + " " + result.SegundoNombre + " " + result.PrimerApellido + " " + result.SegundoApellido;
            var contenido = ObtenerPlantillaCorreo(nombre, nuevaContrasenna);
            _password.EnviarCorreo(result.CorreoElectronico, "Recuperación de Acceso", contenido);
            return Ok(result);
        }


        private static string GenerarContrasenna()
        {
            const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string([.. Enumerable.Range(0, 8).Select(_ => letras[Random.Shared.Next(letras.Length)])]);
        }

        private static string ObtenerPlantillaCorreo(string nombre, string contrasenna)
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Templates", "RecuperarAcceso.html");
            var plantilla = System.IO.File.ReadAllText(ruta);
            return plantilla
                .Replace("{{Nombre}}", nombre)
                .Replace("{{Contrasenna}}", contrasenna);
        }

        private string GenerarToken(string usuarioId)
        {
            var key = Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:Key")!);

            var claims = new[]
            {
                new Claim("UsuarioId", usuarioId.ToString()),
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            var tokenDescriptor = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
