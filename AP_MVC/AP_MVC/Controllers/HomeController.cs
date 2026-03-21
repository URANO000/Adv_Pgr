using System.Net;
using AP_MVC.Filters;
using AP_MVC.Models;
using AP_MVC.Services;
using Microsoft.AspNetCore.Mvc;

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

            if(result.StatusCode == HttpStatusCode.OK)
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
        public IActionResult Login(Usuario model)
        {
            model.Contrasenna = _password.Encrypt(model.Contrasenna);

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Home/IniciarSesion";
            var result = client.PostAsJsonAsync(url, model).Result;

            if(result.StatusCode == HttpStatusCode.OK)
            {
                var objeto = result.Content.ReadFromJsonAsync<Usuario>().Result;
                var NombreCompleto = objeto!.PrimerNombre + " " + objeto!.SegundoNombre + " " + objeto!.PrimerApellido + " " + objeto!.SegundoApellido;
                HttpContext.Session.SetString("NombreUsuario", NombreCompleto);
                HttpContext.Session.SetString("UsuarioId", objeto!.UsuarioId);
                HttpContext.Session.SetString("Token", objeto!.Token);
                HttpContext.Session.SetString(
                    "ImagenPerfil",
                    string.IsNullOrEmpty(objeto!.ImagenPerfil)
                        ? "/uploads/default.jpg"
                        : objeto.ImagenPerfil
                );
                HttpContext.Session.SetInt32("Rol", (int)objeto!.RolId);

                return RedirectToAction("Index", "Home");
            }
            else if(result.StatusCode == HttpStatusCode.InternalServerError)
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

    }
}