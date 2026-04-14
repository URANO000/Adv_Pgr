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
    public class UsuarioController : BaseController
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
        public async Task<IActionResult> CambiarAcceso(Seguridad model)
        {
            var token = ValidarToken(out IActionResult redirect);

            model.NuevaContrasenna = _password.Encrypt(model.NuevaContrasenna);
            model.ConfirmarContrasenna = _password.Encrypt(model.ConfirmarContrasenna);


            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/CambiarAcceso";
            var result = client.PutAsJsonAsync(url, model).Result;

            //Manejo de errores
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }


            if (!result.IsSuccessStatusCode)
            {
                var errorContent = await result.Content.ReadAsStringAsync();

                ViewBag.Mensaje = string.IsNullOrWhiteSpace(errorContent)
                    ? $"Error: {result.StatusCode}"
                    : errorContent;

                return View(model);
            }

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

        #region Perfil
        [HttpGet]
        public async Task<IActionResult> CambiarPerfil()
        {
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
                return redirect;

            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/VerPerfil";

            var result = await client.GetAsync(url);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var objeto = await result.Content.ReadFromJsonAsync<Usuario>();
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
            var token = ValidarToken(out IActionResult redirect);

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
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

            //Por si el token está vacío
            if (!result.IsSuccessStatusCode)
            {
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Home");
                }

                ViewBag.Mensaje = $"Error: {result.StatusCode}";
                return View(model);
            }

            if (result.StatusCode == HttpStatusCode.OK)
            {
                HttpContext.Session.SetString("NombreUsuario", model.nombreCompleto);

                if (ImagenPerfil != null && ImagenPerfil.Length > 0)
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
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
                return redirect;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = _config.GetValue<string>("Valores:UrlAPI") + "Usuario/ListaUsuarios";
            var result = client.GetAsync(url).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var usuarios = result.Content.ReadFromJsonAsync<List<Usuario>>().Result ?? new List<Usuario>();
                return View(usuarios);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion

        #region VerPerfil
        [HttpGet]
        public async Task<IActionResult> Detalle(string usuarioId)
        {
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
            {
                return redirect;
            }

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest("UsuarioId es requerido.");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = _config.GetValue<string>("Valores:UrlAPI") + $"Usuario/VerDetalle/{usuarioId}";
            var result = await client.GetAsync(url);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var objeto = await result.Content.ReadFromJsonAsync<Usuario>();
                return View(objeto);
            }

            return View();
        }
        #endregion

        #region ActivarUsuario
        [HttpPost]
        public async Task<IActionResult> Activar(string usuarioId)
        {
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
                return redirect;

            if (string.IsNullOrWhiteSpace(usuarioId))
                TempData["Error"] = "UsuarioId es requerido.";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = _config.GetValue<string>("Valores:UrlAPI") +
                      $"Usuario/ActivarUsuario/{usuarioId}";

            var response = await client.PutAsync(url, null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var message = await response.Content.ReadAsStringAsync();

            TempData["Success"] = message;

            return RedirectToAction("ListarUsuarios", "Usuario");
        }
        #endregion

        #region DesactivarUsuario
        [HttpPost]
        public async Task<IActionResult> Desactivar(string usuarioId)
        {
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
                return redirect;

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                TempData["Error"] = "UsuarioId es requerido.";
                return RedirectToAction("ListarUsuarios", "Usuario");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = _config.GetValue<string>("Valores:UrlAPI") +
                      $"Usuario/DesactivarUsuario/{usuarioId}";

            var response = await client.PutAsync(url, null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var message = await response.Content.ReadAsStringAsync();

            TempData["Success"] = message;

            return RedirectToAction("ListarUsuarios", "Usuario");
        }
        #endregion

        #region Autorizacion
        [HttpPost]
        public async Task<IActionResult> AuthorizeUN(string usuarioId)
        {
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
                return redirect;

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                TempData["Error"] = "UsuarioId es requerido.";
                return RedirectToAction("ListarUsuarios", "Usuario");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            //API CALL
            var url = _config.GetValue<string>("Valores:UrlAPI") +
                      $"Usuario/AuthorizeUN/{usuarioId}";

            var response = await client.PutAsync(url, null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var message = await response.Content.ReadAsStringAsync();

            TempData["Success"] = message;


            return RedirectToAction("ListarUsuarios", "Usuario");

        }

        [HttpPost]
        public async Task<IActionResult> AuthorizeAD(string usuarioId)
        {
            var token = ValidarToken(out IActionResult redirect);

            if (redirect != null)
                return redirect;

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                TempData["Error"] = "UsuarioId es requerido.";
                return RedirectToAction("ListarUsuarios", "Usuario");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            //API CALL
            var url = _config.GetValue<string>("Valores:UrlAPI") +
                      $"Usuario/AuthorizeAD/{usuarioId}";

            var response = await client.PutAsync(url, null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            var message = await response.Content.ReadAsStringAsync();

            TempData["Success"] = message;


            return RedirectToAction("ListarUsuarios", "Usuario");

        }

        #endregion


    }
}