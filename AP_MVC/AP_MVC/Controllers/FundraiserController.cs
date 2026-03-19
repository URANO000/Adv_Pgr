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
        // Consulta dinámica de animales desde el API
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

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var animales = result.Content.ReadFromJsonAsync<List<Animal>>().Result ?? new List<Animal>();
                return animales.FirstOrDefault(a => a.AnimalId == animalId)?.Nombre ?? string.Empty;
            }

            return string.Empty;
        }
        #endregion

        #region Registrar
        // GET: Fundraiser/Registrar
        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.Animales = GetAnimales();
            return View("RegistrarPublicacionDonacion");
        }

        // POST: Fundraiser/Registrar
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
            {
                throw new Exception();
            }

            ViewBag.Animales = GetAnimales(model.AnimalId);
            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("RegistrarPublicacionDonacion", model);
        }
        #endregion

        #region Editar
        // GET: Fundraiser/Editar/5
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
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("EditarPublicacion");
        }

        // POST: Fundraiser/Editar
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
            {
                throw new Exception();
            }

            ViewBag.Animales = GetAnimales(model.AnimalId);
            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("EditarPublicacion", model);
        }
        #endregion

        #region Detalles
        // GET: Fundraiser/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id = 1)
        {
            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + $"Fundraiser/ObtenerFundraiser/{id}";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content.ReadFromJsonAsync<Fundraiser>().Result;
                ViewBag.Animales = GetAnimales(model!.AnimalId);
                return View("DetalleFundraiser", model);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("DetalleFundraiser");
        }
        #endregion

        #region Inactivar / Cambiar Estado
        // POST: Fundraiser/Inactivar
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
                return RedirectToAction("Catalogo");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Catalogo");
        }
        #endregion

        #region Catálogo
        // GET: Fundraiser/Catalogo
        public IActionResult Catalogo()
        {
            return View("CatalogoDonaciones");
        }
        #endregion
    }
}