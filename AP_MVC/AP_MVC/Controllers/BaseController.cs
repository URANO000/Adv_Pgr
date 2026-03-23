using Microsoft.AspNetCore.Mvc;

namespace AP_MVC.Controllers
{
    public class BaseController : Controller
    {
        #region Helpers
        protected string ValidarToken(out IActionResult redirect)
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                redirect = RedirectToAction("Login", "Home");
                return null;
            }

            redirect = null;
            return token;
        }

        #endregion
    }
}
