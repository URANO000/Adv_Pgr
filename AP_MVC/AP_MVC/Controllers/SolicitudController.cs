using AP_MVC.Filters;
using AP_MVC.Models.Adopciones;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AP_MVC.Controllers
{
    public class SolicitudAdopcionController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        public SolicitudAdopcionController(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private string UrlAPI => _config.GetValue<string>("Valores:UrlAPI")!;

        private string ObtenerUsuarioIdSesion()
            => HttpContext.Session.GetString("UsuarioId") ?? string.Empty;

        // ----------------------------------------------------------------
        // RF-022: Listar solicitudes recibidas de una publicación
        // GET /SolicitudAdopcion/SolicitudesRecibidas?publicacionId=5
        // ----------------------------------------------------------------
        [SesionActiva]
        [HttpGet]
        public IActionResult SolicitudesRecibidas(int publicacionId)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "Sesión expirada.";
                return RedirectToAction("Login", "Home");
            }

            using var client = _http.CreateClient();
            var url = UrlAPI + $"SolicitudAdopcion/ListarSolicitudesPorPublicacion/{publicacionId}/{usuarioId}";
            var result = client.GetAsync(url).Result;

            var solicitudes = new List<SolicitudAdopcionViewModel>();

            if (result.StatusCode == HttpStatusCode.OK)
                solicitudes = result.Content
                    .ReadFromJsonAsync<List<SolicitudAdopcionViewModel>>().Result
                    ?? new List<SolicitudAdopcionViewModel>();
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            ViewBag.PublicacionId = publicacionId;
            return View(solicitudes);
        }
        // RF-023: Consulta solicitudes enviadas por el usuario autenticado
        [SesionActiva]
        [HttpGet]
        public IActionResult SolicitudesEnviadas()
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "Sesión expirada.";
                return RedirectToAction("Login", "Home");
            }

            using var client = _http.CreateClient();
            var url = UrlAPI + $"SolicitudAdopcion/ListarSolicitudesEnviadasPorUsuario/{usuarioId}";
            var result = client.GetAsync(url).Result;

            var solicitudes = new List<SolicitudEnviadaViewModel>();

            if (result.StatusCode == HttpStatusCode.OK)
                solicitudes = result.Content
                    .ReadFromJsonAsync<List<SolicitudEnviadaViewModel>>().Result
                    ?? new List<SolicitudEnviadaViewModel>();
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return View(solicitudes);
        }
        // ----------------------------------------------------------------
        // RF-022 + RF-026: Aprobar o rechazar una solicitud
        // POST /SolicitudAdopcion/Gestionar
        // ----------------------------------------------------------------
        [SesionActiva]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Gestionar(GestionarSolicitudAdopcionViewModel model)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "Sesión expirada.";
                return RedirectToAction("Login", "Home");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos al gestionar la solicitud.";
                return RedirectToAction(nameof(SolicitudesRecibidas),
                    new { publicacionId = model.PublicacionId });
            }

            var payload = new
            {
                model.SolicitudId,
                PropietarioId = usuarioId,
                model.NuevoEstado
            };

            using var client = _http.CreateClient();
            var url = UrlAPI + "SolicitudAdopcion/GestionarSolicitudAdopcion";
            var result = client.PostAsJsonAsync(url, payload).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var accion = model.NuevoEstado switch
                {
                    "Aprobada" => "aprobada",
                    "Rechazada" => "rechazada",
                    "Revisar" => "marcada como En revisión",
                    _ => model.NuevoEstado.ToLower()
                };

                var notificaCorreo = model.NuevoEstado != "Revisar";
                TempData["Exito"] = $"La solicitud fue {accion} correctamente." +
                                    (notificaCorreo ? " El interesado recibirá una notificación por correo." : "");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }
            else
            {
                TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            }

            return RedirectToAction(nameof(SolicitudesRecibidas),
                new { publicacionId = model.PublicacionId });
        }
    }
}