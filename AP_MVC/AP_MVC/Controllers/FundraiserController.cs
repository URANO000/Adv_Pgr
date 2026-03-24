using AP_MVC.Filters;
using AP_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;

namespace AP_MVC.Controllers
{
    public class FundraiserController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        public FundraiserController(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        #region Obtener Animales
        private SelectList GetAnimales(int? selectedId = null)
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Animal/ListarAnimales";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var animales = result.Content.ReadFromJsonAsync<List<Animal>>().Result ?? new List<Animal>();
                return new SelectList(animales, "AnimalId", "Nombre", selectedId);
            }

            return new SelectList(new List<Animal>(), "AnimalId", "Nombre");
        }

        private string GetAnimalNombre(int animalId)
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Animal/ListarAnimales";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var animales = result.Content.ReadFromJsonAsync<List<Animal>>().Result ?? new List<Animal>();
                return animales.FirstOrDefault(a => a.AnimalId == animalId)?.Nombre ?? string.Empty;
            }

            return string.Empty;
        }
        #endregion

        #region Registrar — Evelyn
        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.Animales = GetAnimales();
            return View("RegistrarPublicacionDonacion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(Fundraiser model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Animales = GetAnimales(model.AnimalId);
                return View("RegistrarPublicacionDonacion", model);
            }

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Fundraiser/RegistrarFundraiser";
            var result = client.PostAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "¡Publicación de donación creada exitosamente!";
                return RedirectToAction("Catalogo");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            ViewBag.Animales = GetAnimales(model.AnimalId);
            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("RegistrarPublicacionDonacion", model);
        }
        #endregion

        #region Editar — Evelyn
        [HttpGet]
        public IActionResult Editar(int id = 1)
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + $"Fundraiser/ObtenerFundraiser/{id}";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content.ReadFromJsonAsync<Fundraiser>().Result;

                if (!model!.IsActive)
                {
                    TempData["MostrarModalInactivo"] = true;
                    return RedirectToAction("Registrar");
                }

                ViewBag.AnimalNombre = GetAnimalNombre(model.AnimalId);
                return View("EditarPublicacion", model);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("EditarPublicacion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Fundraiser model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AnimalNombre = GetAnimalNombre(model.AnimalId);
                return View("EditarPublicacion", model);
            }

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Fundraiser/EditarFundraiser";
            var result = client.PutAsJsonAsync(url, model).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "¡Publicación actualizada exitosamente!";
                return RedirectToAction("Catalogo");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            ViewBag.Animales = GetAnimales(model.AnimalId);
            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("EditarPublicacion", model);
        }
        #endregion

        #region Inactivar — Evelyn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Inactivar(int id)
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + $"Fundraiser/InactivarFundraiser/{id}";
            var result = client.PostAsJsonAsync(url, new { }).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La publicación fue inactivada correctamente.";
                return RedirectToAction("MisDonaciones");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("MisDonaciones");
        }
        #endregion

        // ── ISAAC ─────────────────────────────────────────────────────

        #region RF-017: Catálogo de fundraisers activos
        public IActionResult Catalogo()
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Fundraiser/ListarFundraisers";
            var result = client.GetAsync(url).Result;

            var fundraisers = new List<Fundraiser>();

            if (result.StatusCode == HttpStatusCode.OK)
                fundraisers = result.Content.ReadFromJsonAsync<List<Fundraiser>>().Result ?? new List<Fundraiser>();

            return View("CatalogoDonaciones", fundraisers);
        }
        #endregion

        #region RF-017: Detalle + donaciones recibidas
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            using var client = _http.CreateClient();
            var urlFundraiser = _config.GetValue<string>("Valores:UrlAPI") + $"Fundraiser/ObtenerFundraiser/{id}";
            var result = client.GetAsync(urlFundraiser).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content.ReadFromJsonAsync<Fundraiser>().Result;

                // Donaciones del fundraiser
                var urlDonaciones = _config.GetValue<string>("Valores:UrlAPI") + $"Fundraiser/ListarDonaciones/{id}";
                var resDonaciones = client.GetAsync(urlDonaciones).Result;
                ViewBag.Donaciones = resDonaciones.StatusCode == HttpStatusCode.OK
                    ? resDonaciones.Content.ReadFromJsonAsync<List<Donacion>>().Result ?? new List<Donacion>()
                    : new List<Donacion>();

                ViewBag.EstaLogueado = !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
                return View("DetalleFundraiser", model);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Catalogo");
        }
        #endregion

        #region RF-020: Simulación de donación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Donar(int fundraiserId, decimal monto, string metodoPago)
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");

            var payload = new
            {
                FundraiserId = fundraiserId,
                UsuarioId    = usuarioId,
                Total        = monto,
                MetodoPago   = metodoPago
            };

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Fundraiser/RealizarDonacion";
            var result = client.PostAsJsonAsync(url, payload).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["DonacionExito"] = $"PS-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
                TempData["MetodoPago"]    = metodoPago;
                TempData["MontoDonado"]   = monto.ToString("N2");
                return RedirectToAction("Detalle", new { id = fundraiserId });
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Detalle", new { id = fundraiserId });
        }
        #endregion

        #region RF-022: Historial (activos + inactivos) del usuario logueado
        [SesionActiva]
        public IActionResult MisDonaciones()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + $"Fundraiser/ListarHistorial/{usuarioId}";
            var result = client.GetAsync(url).Result;

            var fundraisers = new List<Fundraiser>();

            if (result.StatusCode == HttpStatusCode.OK)
                fundraisers = result.Content.ReadFromJsonAsync<List<Fundraiser>>().Result ?? new List<Fundraiser>();

            return View("MisDonaciones", fundraisers);
        }
        #endregion
    }
}