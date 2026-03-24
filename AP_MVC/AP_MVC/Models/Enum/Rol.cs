using System.ComponentModel.DataAnnotations;

namespace AP_MVC.Models.Enum
{
    public enum Rol
    {
        [Display(Name = "Administrador")]
        Administrador = 1,

        [Display(Name = "Usuario Normal")]
        UsuarioNormal = 2
    }
}
