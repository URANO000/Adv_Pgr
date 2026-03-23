using Dapper;
using AP_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AP_WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundraiserController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FundraiserController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("ObtenerFundraiser/{id}")]
        public IActionResult ObtenerFundraiser(int id)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@FundraiserId", id);

            var result = context.QueryFirstOrDefault<FundraiserResponse>("sp_ObtenerFundraiser", parametros);

            if (result == null)
                return NotFound("La publicación no fue encontrada");


            return Ok(result);
        }

        [HttpPost("RegistrarFundraiser")]
        public IActionResult RegistrarFundraiser(RegistrarFundraiserRequest model)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@AnimalId", model.AnimalId);
            parametros.Add("@Titulo", model.Titulo);
            parametros.Add("@Descripcion", model.Descripcion);
            parametros.Add("@MetaTotal", model.MetaTotal);

            var result = context.Execute("sp_RegistrarFundraiser", parametros);

            if (result <= 0)
                return BadRequest("La publicación no se registró correctamente");

            return Ok("La publicación se registró correctamente");
        }

        [HttpPut("EditarFundraiser")]
        public IActionResult EditarFundraiser(EditarFundraiserRequest model)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@FundraiserId", model.FundraiserId);
            parametros.Add("@AnimalId", model.AnimalId);
            parametros.Add("@Titulo", model.Titulo);
            parametros.Add("@Descripcion", model.Descripcion);
            parametros.Add("@MetaTotal", model.MetaTotal);

            var result = context.Execute("sp_EditarFundraiser", parametros);

            if (result <= 0)
                return BadRequest("La publicación no se actualizó correctamente");

            return Ok("La publicación se actualizó correctamente");
        }

        [HttpPost("InactivarFundraiser/{id}")]
        public IActionResult InactivarFundraiser(int id)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@FundraiserId", id);

            var result = context.Execute("sp_InactivarFundraiser", parametros);

            if (result <= 0)
                return BadRequest("La publicación no se pudo inactivar o ya estaba inactiva");

            return Ok("La publicación se inactivó correctamente");
        }
    }
}