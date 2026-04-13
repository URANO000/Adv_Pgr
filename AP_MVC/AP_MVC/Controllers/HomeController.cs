using AP_MVC.Filters;
using AP_MVC.Models;
using AP_MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace AP_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;
        private readonly IPasswordHelper _password;

        public HomeController(IHttpClientFactory http, IConfiguration config, IPasswordHelper password)
        {
            _http = http;
            _config = config;
            _password = password;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region Crear Cuenta
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario model)
        {
            model.Contrasenna = _password.Encrypt(model.Contrasenna);

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Home/RegistrarUsuario";
            var result = client.PostAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Login", "Home");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }
        #endregion

        #region Inicio de sesión

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Usuario model)
        {
            model.Contrasenna = _password.Encrypt(model.Contrasenna);

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Home/IniciarSesion";
            var result = client.PostAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var objeto = result.Content.ReadFromJsonAsync<Usuario>().Result;

                //Convierto JWT a claims
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(objeto!.Token);

                var claims = jwt.Claims.ToList();

                //Verificar que el rol sirve
                var roleClaim = claims.FirstOrDefault(c => c.Type.Contains("role"));
                if (roleClaim != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                }

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                //Lo demás de la UI
                HttpContext.Session.SetString("NombreUsuario", objeto!.nombreCompleto);
                HttpContext.Session.SetString("UsuarioId", objeto!.UsuarioId);
                HttpContext.Session.SetString("Token", objeto!.Token);
                HttpContext.Session.SetString(
                    "ImagenPerfil",
                    string.IsNullOrEmpty(objeto!.ImagenPerfil)
                        ? "/uploads/default.jpg"
                        : objeto.ImagenPerfil
                );

                return RedirectToAction("Index", "Home");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion

        #region Cerrar Sesión

        [SesionActiva]
        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        #endregion

        #region Recuperar Acceso

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarAcceso(Usuario model)
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Home/RecuperarAcceso";
            var result = client.PutAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Login", "Home");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion

        #region Misc
        [HttpGet]
        public IActionResult Contacto()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Nosotros()
        {
            return View();
        }

        #endregion


    }
}