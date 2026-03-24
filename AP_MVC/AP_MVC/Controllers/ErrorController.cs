using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AP_MVC.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult CapturarError()
        {
            //Retorna la excepcion detalladamente (para saber cómo arreglarlo)
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            return View("Error");
        }
    }
}
