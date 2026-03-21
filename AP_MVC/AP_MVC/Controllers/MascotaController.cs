using AP_MVC.Filters;
using AP_MVC.Models.Mascotas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;

namespace AP_MVC.Controllers
{
    [SesionActiva]
    public class MascotaController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        public MascotaController(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private string UrlAPI => _config.GetValue<string>("Valores:UrlAPI")!;

        private string ObtenerUsuarioIdSesion()
        {
            return HttpContext.Session.GetString("UsuarioId") ?? string.Empty;
        }

        private SelectList GetTiposAnimal(int? selectedId = null)
        {
            using var client = _http.CreateClient();

            var url = UrlAPI + "Animal/ListarTiposAnimal";

            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var lista = result.Content
                    .ReadFromJsonAsync<List<AnimalTipoViewModel>>().Result
                    ?? new List<AnimalTipoViewModel>();

                return new SelectList(lista, "TipoId", "Nombre", selectedId);
            }

            return new SelectList(new List<AnimalTipoViewModel>(), "TipoId", "Nombre");
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.TiposAnimal = GetTiposAnimal();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(MascotaPublicacionCrearViewModel model)
        {
            model.UsuarioId = ObtenerUsuarioIdSesion();

            if (!ModelState.IsValid)
            {
                ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);
                return View(model);
            }

            using var client = _http.CreateClient();
            var url = UrlAPI + "Mascota/RegistrarPublicacionMascota";
            var result = client.PostAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La publicación de mascota se registró correctamente.";
                return RedirectToAction("MisPublicaciones");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);
            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(model);
        }

        [HttpGet]
        public IActionResult MisPublicaciones()
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            using var client = _http.CreateClient();
            var url = UrlAPI + $"Mascota/ListarMisPublicacionesMascota/{usuarioId}";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content.ReadFromJsonAsync<List<MascotaPublicacionViewModel>>().Result
                            ?? new List<MascotaPublicacionViewModel>();

                return View(model);
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.Mensaje = "No tienes publicaciones registradas.";
                return View(new List<MascotaPublicacionViewModel>());
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(new List<MascotaPublicacionViewModel>());
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            using var client = _http.CreateClient();
            var url = UrlAPI + $"Mascota/ObtenerPublicacionMascota/{id}/{usuarioId}";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var data = result.Content.ReadFromJsonAsync<MascotaPublicacionViewModel>().Result;

                if (data == null)
                {
                    TempData["Error"] = "No se encontró la publicación.";
                    return RedirectToAction("MisPublicaciones");
                }

                var model = new MascotaPublicacionEditarViewModel
                {
                    PublicacionId = data.PublicacionId,
                    UsuarioId = usuarioId,
                    Titulo = data.Titulo,
                    Descripcion = data.Descripcion
                };

                return View(model);
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "No se encontró la publicación.";
                return RedirectToAction("MisPublicaciones");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("MisPublicaciones");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(MascotaPublicacionEditarViewModel model)
        {
            model.UsuarioId = ObtenerUsuarioIdSesion();

            if (!ModelState.IsValid)
                return View(model);

            using var client = _http.CreateClient();
            var url = UrlAPI + "Mascota/EditarPublicacionMascota";
            var result = client.PutAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La publicación se actualizó correctamente.";
                return RedirectToAction("MisPublicaciones");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Inactivar(int id)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            using var client = _http.CreateClient();
            var url = UrlAPI + $"Mascota/InactivarPublicacionMascota/{id}/{usuarioId}";
            var result = client.PostAsJsonAsync(url, new { }).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La publicación se inactivó correctamente.";
                return RedirectToAction("MisPublicaciones");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("MisPublicaciones");
        }

        [HttpGet]
        public IActionResult Media(int animalId)
        {
            using var client = _http.CreateClient();
            var url = UrlAPI + $"Mascota/ListarAnimalMedia/{animalId}";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content.ReadFromJsonAsync<List<AnimalMediaViewModel>>().Result
                            ?? new List<AnimalMediaViewModel>();

                ViewBag.AnimalId = animalId;
                return View(model);
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.AnimalId = animalId;
                ViewBag.Mensaje = "No hay imágenes registradas.";
                return View(new List<AnimalMediaViewModel>());
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.AnimalId = animalId;
            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(new List<AnimalMediaViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarMedia(RegistrarAnimalMediaViewModel model)
        {
            model.CreatedBy = ObtenerUsuarioIdSesion();

            if (!ModelState.IsValid)
                return RedirectToAction("Media", new { animalId = model.AnimalId });

            using var client = _http.CreateClient();
            var url = UrlAPI + "Mascota/RegistrarAnimalMedia";
            var result = client.PostAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La imagen se registró correctamente.";
                return RedirectToAction("Media", new { animalId = model.AnimalId });
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Media", new { animalId = model.AnimalId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarMedia(int mediaId, int animalId)
        {
            var model = new EliminarAnimalMediaViewModel
            {
                MediaId = mediaId,
                UsuarioId = ObtenerUsuarioIdSesion()
            };

            using var client = _http.CreateClient();
            var url = UrlAPI + "Mascota/EliminarAnimalMedia";
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = JsonContent.Create(model)
            };

            var result = client.SendAsync(request).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La imagen se eliminó correctamente.";
                return RedirectToAction("Media", new { animalId });
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Media", new { animalId });
        }
    }
}