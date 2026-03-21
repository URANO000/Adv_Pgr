using AP_MVC.Filters;
using AP_MVC.Models;
using AP_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace AP_MVC.Controllers
{
    [SesionActiva]
    public class UsuarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IPasswordHelper _password;
        public UsuarioController(IHttpClientFactory http, IConfiguration config, IWebHostEnvironment env, IPasswordHelper password)
        {
            _http = http;
            _config = config;
            _env = env;
            _password = password;
        }

        #region CambiarAcceso
        [HttpGet]
        public IActionResult CambiarAcceso()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CambiarAcceso(Seguridad model)
        {
            model.NuevaContrasenna = _password.Encrypt(model.NuevaContrasenna);
            model.ConfirmarContrasenna = _password.Encrypt(model.ConfirmarContrasenna);

            var token = HttpContext.Session.GetString("Token");

            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/CambiarAcceso";
            var result = client.PutAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("CerrarSesion", "Home");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }
        

        #endregion

            #region Perfil
            [HttpGet]
        public async Task<IActionResult> CambiarPerfil()
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("CerrarSesion", "Home");
            }


            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/VerPerfil";

            var result = await client.GetAsync(url);
            var objeto = await result.Content.ReadFromJsonAsync<Usuario>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return View(objeto);
            }

            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = await result.Content.ReadAsStringAsync();
            return View();
        }

        [HttpPost]
        public IActionResult CambiarPerfil(Usuario model, IFormFile? ImagenPerfil)
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("CerrarSesion", "Home");
            }


            var UsuarioId = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(model.ImagenPerfil))
            {
                model.ImagenPerfil = HttpContext.Session.GetString("ImagenPerfil") ?? "";
            }

            if (ImagenPerfil != null && ImagenPerfil.Length > 0)
            {

                var extensionesPermitidas = new[] { ".png", ".jpg", ".jpeg", ".gif" };
                var tiposPermitidos = new[] { "image/png", "image/jpeg", "image/gif" };

                var extension = Path.GetExtension(ImagenPerfil.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension) ||
                    !tiposPermitidos.Contains(ImagenPerfil.ContentType))
                {
                    ViewBag.Mensaje = "Formato de imagen no permitido.";
                    return View(model);
                }

                var folder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Si la extensión era diferente, borrar la img anterior
                var archivosExistentes = Directory.GetFiles(folder, $"{UsuarioId}.*");

                foreach (var archivo in archivosExistentes)
                {
                    System.IO.File.Delete(archivo);
                }


                var nombreArchivo = $"{UsuarioId}{extension}";
                var rutaCompleta = Path.Combine(folder, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                ImagenPerfil.CopyTo(stream);

                model.ImagenPerfil = $"/uploads/{nombreArchivo}?v={DateTime.Now.Ticks}";
            }



            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/EditarPerfil";
            var result = client.PutAsJsonAsync(url, model).Result;

            if(result.StatusCode == HttpStatusCode.OK)
            {
                var NombreCompleto = model!.PrimerNombre + " " + model!.SegundoNombre + " " + model!.PrimerApellido + " " + model!.SegundoApellido;
                HttpContext.Session.SetString("NombreUsuario", NombreCompleto);

                if(ImagenPerfil != null && ImagenPerfil.Length > 0)
                {
                    HttpContext.Session.SetString("ImagenPerfil", model.ImagenPerfil);

                    ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
                    return View(model);
                }
                else if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception();
                }
            }


            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(model);

        }

        #endregion

        #region ListaUsuarios
        [Authorize(Roles = "Administrador")]
        public IActionResult ListarUsuarios()
        {
            var token = HttpContext.Session.GetString("Token");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/ListaUsuarios";
            var result = client.GetAsync(url).Result;
            if(result.StatusCode == HttpStatusCode.OK)
            {
                var usuarios = result.Content.ReadFromJsonAsync<List<Usuario>>().Result ?? new List<Usuario>();
                return View(usuarios);
            }
            else if(result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion

        #region VerPerfil


        #endregion


    }
}
