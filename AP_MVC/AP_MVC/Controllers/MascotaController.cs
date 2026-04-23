using AP_MVC.Filters;
using AP_MVC.Models;
using AP_MVC.Models.Adopciones;
using AP_MVC.Models.Mascotas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;

namespace AP_MVC.Controllers
{
    public class MascotaController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public MascotaController(IHttpClientFactory http, IConfiguration config, IWebHostEnvironment env)
        {
            _http = http;
            _config = config;
            _env = env;
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

                return new SelectList(lista, "TipoId", "NombreTipo", selectedId);
            }

            return new SelectList(new List<AnimalTipoViewModel>(), "TipoId", "NombreTipo");
        }
        [SesionActiva]
        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.TiposAnimal = GetTiposAnimal();
            return View();
        }
        [SesionActiva]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(MascotaPublicacionCrearViewModel model, List<IFormFile> ImagenesArchivos)
        {

            List<(string ruta, long size)> archivosGuardados = new();

            if (ImagenesArchivos != null && ImagenesArchivos.Count > 0)
            {
                foreach (var archivo in ImagenesArchivos)
                {
                    if (archivo.Length == 0)
                        continue;

                    var extension = Path.GetExtension(archivo.FileName).ToLower();

                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                    if (!extensionesPermitidas.Contains(extension))
                        continue;

                    var nombreArchivo = Guid.NewGuid().ToString() + extension;

                    var carpetaDestino = Path.Combine(
                        _env.WebRootPath,
                        "uploads",
                        "mascotas"
                    );

                    if (!Directory.Exists(carpetaDestino))
                    {
                        Directory.CreateDirectory(carpetaDestino);
                    }

                    var rutaFisica = Path.Combine(carpetaDestino, nombreArchivo);

                    using (var stream = new FileStream(rutaFisica, FileMode.Create))
                    {
                        archivo.CopyTo(stream);
                    }

                    var rutaRelativa = "/uploads/mascotas/" + nombreArchivo;

                    archivosGuardados.Add((rutaRelativa, archivo.Length));
                }
            }

            model.UsuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(model.UsuarioId))
            {
                ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);
                ViewBag.Mensaje = "La sesión expiró o no contiene un UsuarioId válido.";
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);
                return View(model);
            }

            using var client = _http.CreateClient();

            var urlRegistrar = UrlAPI + "Mascota/RegistrarPublicacionMascota";
            var responseRegistrar = client.PostAsJsonAsync(urlRegistrar, model).Result;

            if (!responseRegistrar.IsSuccessStatusCode)
            {
                ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);
                ViewBag.Mensaje = responseRegistrar.Content.ReadAsStringAsync().Result;
                return View(model);
            }

            var registro = responseRegistrar.Content
                .ReadFromJsonAsync<RegistrarPublicacionMascotaResultViewModel>()
                .Result;

            if (registro == null || registro.AnimalId <= 0 || registro.PublicacionId <= 0)
            {
                ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);
                ViewBag.Mensaje = "La publicación se registró, pero no se obtuvo la información completa.";
                return View(model);
            }

            if (archivosGuardados.Count > 0)
            {
                foreach (var img in archivosGuardados)
                {
                    var mediaRequest = new RegistrarAnimalMediaViewModel
                    {
                        AnimalId = registro.AnimalId,
                        ArchivoUrl = img.ruta,
                        FileSize = img.size,
                        CreatedBy = model.UsuarioId
                    };

                    var urlMedia = UrlAPI + "Mascota/RegistrarAnimalMedia";

                    var responseMedia =
                        client.PostAsJsonAsync(urlMedia, mediaRequest).Result;

                    if (!responseMedia.IsSuccessStatusCode)
                    {
                        ViewBag.TiposAnimal = GetTiposAnimal(model.TipoId);

                        ViewBag.Mensaje =
                            "Error registrando una imagen: "
                            + responseMedia.Content.ReadAsStringAsync().Result;

                        return View(model);
                    }
                }
            }

            TempData["Exito"] = "La publicación de mascota se registró correctamente.";
            return RedirectToAction("Detalle", new { id = registro.PublicacionId });
        }

        [HttpGet]
        [SesionActiva]
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

                foreach (var item in model)
                {
                    var urlMedia = UrlAPI + $"Mascota/ListarAnimalMedia/{item.AnimalId}";
                    var resultMedia = client.GetAsync(urlMedia).Result;

                    if (resultMedia.StatusCode == HttpStatusCode.OK)
                    {
                        var imagenes = resultMedia.Content.ReadFromJsonAsync<List<AnimalMediaViewModel>>().Result
                                       ?? new List<AnimalMediaViewModel>();

                        var primeraImagen = imagenes.FirstOrDefault();

                        if (primeraImagen != null && !string.IsNullOrWhiteSpace(primeraImagen.ArchivoUrl))
                        {
                            item.ImagenUrl = primeraImagen.ArchivoUrl.StartsWith("/")
                                ? primeraImagen.ArchivoUrl
                                : "/" + primeraImagen.ArchivoUrl;
                        }
                    }
                }

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
        [SesionActiva]
        public IActionResult Detalle(int id)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "La sesión expiró o no contiene un UsuarioId válido.";
                return RedirectToAction("Login", "Home");
            }

            using var client = _http.CreateClient();

            var urlPublicacion = UrlAPI + $"Mascota/ObtenerPublicacionMascota/{id}/{usuarioId}";
            var resultPublicacion = client.GetAsync(urlPublicacion).Result;

            if (resultPublicacion.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "No se encontró la publicación.";
                return RedirectToAction("MisPublicaciones");
            }
            else if (resultPublicacion.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }
            else if (resultPublicacion.StatusCode != HttpStatusCode.OK)
            {
                TempData["Error"] = resultPublicacion.Content.ReadAsStringAsync().Result;
                return RedirectToAction("MisPublicaciones");
            }

            var publicacion = resultPublicacion.Content
                .ReadFromJsonAsync<MascotaDetalleViewModel>()
                .Result;

            if (publicacion == null)
            {
                TempData["Error"] = "No se pudo cargar el detalle de la publicación.";
                return RedirectToAction("MisPublicaciones");
            }

            var urlMedia = UrlAPI + $"Mascota/ListarAnimalMedia/{publicacion.AnimalId}";
            var resultMedia = client.GetAsync(urlMedia).Result;

            if (resultMedia.StatusCode == HttpStatusCode.OK)
            {
                publicacion.Imagenes = resultMedia.Content
                    .ReadFromJsonAsync<List<AnimalMediaViewModel>>()
                    .Result ?? new List<AnimalMediaViewModel>();
            }
            else if (resultMedia.StatusCode == HttpStatusCode.NotFound)
            {
                publicacion.Imagenes = new List<AnimalMediaViewModel>();
            }
            else if (resultMedia.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }
            else
            {
                publicacion.Imagenes = new List<AnimalMediaViewModel>();
            }

            return View(publicacion);
        }

        [HttpGet]
        [SesionActiva]
        public IActionResult Editar(int id)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "La sesión expiró o no contiene un UsuarioId válido.";
                return RedirectToAction("Login", "Home");
            }

            using var client = _http.CreateClient();

            var url = UrlAPI + $"Mascota/ObtenerPublicacionMascota/{id}/{usuarioId}";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "No se encontró la publicación.";
                return RedirectToAction("MisPublicaciones");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }
            else if (result.StatusCode != HttpStatusCode.OK)
            {
                TempData["Error"] = result.Content.ReadAsStringAsync().Result;
                return RedirectToAction("MisPublicaciones");
            }

            var publicacion = result.Content.ReadFromJsonAsync<MascotaDetalleViewModel>().Result;

            if (publicacion == null)
            {
                TempData["Error"] = "No se pudo cargar la publicación.";
                return RedirectToAction("MisPublicaciones");
            }

            if (!publicacion.IsActive)
            {
                TempData["Error"] = "No se puede editar una publicación inactiva.";
                return RedirectToAction("Detalle", new { id });
            }

            var model = new EditarPublicacionMascotaViewModel
            {
                PublicacionId = publicacion.PublicacionId,
                UsuarioId = usuarioId,
                Titulo = publicacion.Titulo,
                Descripcion = publicacion.Descripcion,
                IsActive = publicacion.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [SesionActiva]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(EditarPublicacionMascotaViewModel model)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "La sesión expiró o no contiene un UsuarioId válido.";
                return RedirectToAction("Login", "Home");
            }

            model.UsuarioId = usuarioId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var client = _http.CreateClient();

            var url = UrlAPI + "Mascota/EditarPublicacionMascota";

            var request = new
            {
                PublicacionId = model.PublicacionId,
                UsuarioId = model.UsuarioId,
                Titulo = model.Titulo,
                Descripcion = model.Descripcion
            };

            var result = client.PutAsJsonAsync(url, request).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La publicación se actualizó correctamente.";
                return RedirectToAction("Detalle", new { id = model.PublicacionId });
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(model);
        }

        [HttpPost]
        [SesionActiva]
        [ValidateAntiForgeryToken]
        public IActionResult Inactivar(int id)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "La sesión expiró o no contiene un UsuarioId válido.";
                return RedirectToAction("Login", "Home");
            }

            using var client = _http.CreateClient();
            var url = UrlAPI + $"Mascota/InactivarPublicacionMascota/{id}/{usuarioId}";
            var result = client.PostAsJsonAsync(url, new { }).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "La publicación se inactivó correctamente.";
                return RedirectToAction("Detalle", new { id });
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Detalle", new { id });
        }

        [HttpGet]
        [SesionActiva]
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
        [SesionActiva]
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
        [SesionActiva]
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

        private SelectList GetTiposAnimalConTodos(int? selectedId = null)
        {
            using var client = _http.CreateClient();

            var url = UrlAPI + "Animal/ListarTiposAnimal";
            var result = client.GetAsync(url).Result;

            var lista = new List<AnimalTipoViewModel>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                lista = result.Content
                    .ReadFromJsonAsync<List<AnimalTipoViewModel>>().Result
                    ?? new List<AnimalTipoViewModel>();
            }

            lista.Insert(0, new AnimalTipoViewModel
            {
                TipoId = 0,
                NombreTipo = "Todos"
            });

            return new SelectList(lista, "TipoId", "NombreTipo", selectedId);
        }

        private SelectList GetCategoriasCatalogo(int? selectedId = null)
        {
            using var client = _http.CreateClient();

            var url = UrlAPI + "Mascota/ListarCategoriasCatalogo";
            var result = client.GetAsync(url).Result;

            var lista = new List<CategoriaCatalogoViewModel>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                lista = result.Content
                    .ReadFromJsonAsync<List<CategoriaCatalogoViewModel>>().Result
                    ?? new List<CategoriaCatalogoViewModel>();
            }

            lista.Insert(0, new CategoriaCatalogoViewModel
            {
                CategoriaId = 0,
                CategoriaAnimal = "Todas"
            });

            return new SelectList(lista, "CategoriaId", "CategoriaAnimal", selectedId);
        }

        private List<MascotaPublicacionViewModel> CargarImagenPrincipalListado(List<MascotaPublicacionViewModel> model)
        {
            using var client = _http.CreateClient();

            foreach (var item in model)
            {
                var urlMedia = UrlAPI + $"Mascota/ListarAnimalMedia/{item.AnimalId}";
                var resultMedia = client.GetAsync(urlMedia).Result;

                if (resultMedia.StatusCode == HttpStatusCode.OK)
                {
                    var imagenes = resultMedia.Content
                        .ReadFromJsonAsync<List<AnimalMediaViewModel>>().Result
                        ?? new List<AnimalMediaViewModel>();

                    var primeraImagen = imagenes.FirstOrDefault();

                    if (primeraImagen != null && !string.IsNullOrWhiteSpace(primeraImagen.ArchivoUrl))
                    {
                        item.ImagenUrl = primeraImagen.ArchivoUrl.StartsWith("/")
                            ? primeraImagen.ArchivoUrl
                            : "/" + primeraImagen.ArchivoUrl;
                    }
                }
            }

            return model;
        }

        [HttpGet]
        public IActionResult Catalogo(string? texto = null, int? tipoId = null, int? categoriaId = null, string? tipo = null)
        {
            if ((!tipoId.HasValue || tipoId.Value == 0) && !string.IsNullOrWhiteSpace(tipo))
            {
                using var clientTipos = _http.CreateClient();
                var urlTipos = UrlAPI + "Animal/ListarTiposAnimal";
                var resultTipos = clientTipos.GetAsync(urlTipos).Result;

                if (resultTipos.StatusCode == HttpStatusCode.OK)
                {
                    var tipos = resultTipos.Content
                        .ReadFromJsonAsync<List<AnimalTipoViewModel>>().Result
                        ?? new List<AnimalTipoViewModel>();

                    var tipoEncontrado = tipos.FirstOrDefault(t =>
                        !string.IsNullOrWhiteSpace(t.NombreTipo) &&
                        t.NombreTipo.Equals(tipo, StringComparison.OrdinalIgnoreCase));

                    if (tipoEncontrado != null)
                    {
                        tipoId = tipoEncontrado.TipoId;
                    }
                }
            }

            using var client = _http.CreateClient();

            var query = $"?texto={Uri.EscapeDataString(texto ?? string.Empty)}";

            if (tipoId.HasValue && tipoId.Value > 0)
            {
                query += $"&tipoId={tipoId.Value}";
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query += $"&categoriaId={categoriaId.Value}";
            }

            var url = UrlAPI + "Mascota/ListarCatalogoMascotas" + query;
            var result = client.GetAsync(url).Result;

            ViewBag.TiposAnimal = GetTiposAnimalConTodos(tipoId);
            ViewBag.Categorias = GetCategoriasCatalogo(categoriaId);
            ViewBag.Texto = texto;
            ViewBag.TipoSeleccionado = tipoId;
            ViewBag.CategoriaSeleccionada = categoriaId;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content
                    .ReadFromJsonAsync<List<MascotaPublicacionViewModel>>().Result
                    ?? new List<MascotaPublicacionViewModel>();

                model = CargarImagenPrincipalListado(model);

                return View(model);
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.Mensaje = "No se encontraron mascotas disponibles.";
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
        public IActionResult BuscarFiltrar(string? texto = null, int? tipoId = null, int? categoriaId = null)
        {
            return RedirectToAction("Catalogo", new { texto, tipoId, categoriaId });
        }

        [HttpGet]
        public IActionResult DetalleCatalogo(int id)
        {
            using var client = _http.CreateClient();

            var urlPublicacion = UrlAPI + $"Mascota/ObtenerDetalleCatalogoMascota/{id}";
            var resultPublicacion = client.GetAsync(urlPublicacion).Result;

            if (resultPublicacion.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "No se encontró la publicación.";
                return RedirectToAction("Catalogo");
            }
            else if (resultPublicacion.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }
            else if (resultPublicacion.StatusCode != HttpStatusCode.OK)
            {
                TempData["Error"] = resultPublicacion.Content.ReadAsStringAsync().Result;
                return RedirectToAction("Catalogo");
            }

            var publicacion = resultPublicacion.Content
                .ReadFromJsonAsync<MascotaDetalleViewModel>()
                .Result;

            if (publicacion == null)
            {
                TempData["Error"] = "No se pudo cargar el detalle de la publicación.";
                return RedirectToAction("Catalogo");
            }

            var urlMedia = UrlAPI + $"Mascota/ListarAnimalMedia/{publicacion.AnimalId}";
            var resultMedia = client.GetAsync(urlMedia).Result;

            if (resultMedia.StatusCode == HttpStatusCode.OK)
            {
                publicacion.Imagenes = resultMedia.Content
                    .ReadFromJsonAsync<List<AnimalMediaViewModel>>()
                    .Result ?? new List<AnimalMediaViewModel>();
            }
            else if (resultMedia.StatusCode == HttpStatusCode.NotFound)
            {
                publicacion.Imagenes = new List<AnimalMediaViewModel>();
            }
            else if (resultMedia.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }
            else
            {
                publicacion.Imagenes = new List<AnimalMediaViewModel>();
            }

            var usuarioIdSesion = HttpContext.Session.GetString("UsuarioId");
            var nombreSesion = HttpContext.Session.GetString("NombreUsuario");
            var tokenSesion = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(usuarioIdSesion))
            {
                publicacion.NombreUsuarioSesion = nombreSesion ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(tokenSesion))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenSesion);
                }

                var urlUsuario = UrlAPI + $"Usuario/VerDetalle/{usuarioIdSesion}";
                var resultUsuario = client.GetAsync(urlUsuario).Result;

                if (resultUsuario.StatusCode == HttpStatusCode.OK)
                {
                    var usuario = resultUsuario.Content.ReadFromJsonAsync<Usuario>().Result;

                    if (usuario != null)
                    {
                        publicacion.CorreoUsuarioSesion = usuario.CorreoElectronico ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(publicacion.NombreUsuarioSesion))
                        {
                            publicacion.NombreUsuarioSesion =
                                $"{usuario.PrimerNombre} {usuario.PrimerApellido}".Trim();
                        }
                    }
                }
                else
                {
                    publicacion.CorreoUsuarioSesion = string.Empty;
                }
            }

            return View("Detalle", publicacion);
        }

        [HttpPost]
        [SesionActiva]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarSolicitudAdopcion(CrearSolicitudAdopcionViewModel model)
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "Debes iniciar sesión para enviar una solicitud.";
                return RedirectToAction("Login", "Home");
            }

            model.UsuarioInteresadoId = usuarioId;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debes escribir un mensaje válido para enviar la solicitud.";
                return RedirectToAction("DetalleCatalogo", new { id = model.PublicacionId });
            }

            using var client = _http.CreateClient();
            var url = UrlAPI + "Mascota/RegistrarSolicitudAdopcion";

            var request = new
            {
                PublicacionId = model.PublicacionId,
                UsuarioInteresadoId = model.UsuarioInteresadoId,
                Mensaje = model.Mensaje
            };

            var result = client.PostAsJsonAsync(url, request).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["Exito"] = "Tu solicitud fue enviada correctamente al propietario.";
                return RedirectToAction("DetalleCatalogo", new { id = model.PublicacionId });
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                TempData["Error"] = result.Content.ReadAsStringAsync().Result;
                return RedirectToAction("DetalleCatalogo", new { id = model.PublicacionId });
            }

            TempData["Error"] = result.Content.ReadAsStringAsync().Result;
            return RedirectToAction("DetalleCatalogo", new { id = model.PublicacionId });
        }

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
            var url = UrlAPI + $"Mascota/ListarSolicitudesPorPublicacion/{publicacionId}/{usuarioId}";
            var result = client.GetAsync(url).Result;

            var solicitudes = new List<SolicitudAdopcionViewModel>();

            if (result.StatusCode == HttpStatusCode.OK)
                solicitudes = result.Content
                    .ReadFromJsonAsync<List<SolicitudAdopcionViewModel>>().Result
                    ?? new List<SolicitudAdopcionViewModel>();
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            ViewBag.PublicacionId = publicacionId;
            return View("SolicitudesRecibidas", solicitudes);
        }

        [HttpGet]
        public IActionResult Historial(string estado = "todas")
        {
            var usuarioId = ObtenerUsuarioIdSesion();

            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "La sesión expiró o no contiene un UsuarioId válido.";
                return RedirectToAction("Login", "Home");
            }

            using var client = _http.CreateClient();

            var url = UrlAPI + $"Mascota/ListarHistorialPublicacionesMascota/{usuarioId}?estado={estado}";
            var result = client.GetAsync(url).Result;

            ViewBag.Estado = estado;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var model = result.Content
                    .ReadFromJsonAsync<List<MascotaPublicacionViewModel>>().Result
                    ?? new List<MascotaPublicacionViewModel>();

                model = CargarImagenPrincipalListado(model);

                return View("MisPublicaciones", model);
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.Mensaje = "No tienes publicaciones registradas.";
                return View("MisPublicaciones", new List<MascotaPublicacionViewModel>());
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View("MisPublicaciones", new List<MascotaPublicacionViewModel>());
        }
    }
}