using Microsoft.AspNetCore.Mvc;

namespace AP_MVC.Controllers
{
    public class BaseController : Controller
    {
        #region Helpers
        protected string ValidarToken(out IActionResult redirect)
        {
            //Si está logged in
            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                redirect = null;
                return token;
            }

            //En caso de remember me
            var user = HttpContext.User;

            if (user == null || !user.Identity!.IsAuthenticated)
            {
                redirect = RedirectToAction("Login", "Home");
                return null;
            }

            //Recuperar de claims
            token = user.FindFirst("Token")?.Value;

            if (string.IsNullOrEmpty(token))
            {
                redirect = RedirectToAction("Login", "Home");
                return null;
            }

            //Restaurar token en session
            HttpContext.Session.SetString("Token", token);

            redirect = null;
            return token;
        }

        #endregion
    }
}
