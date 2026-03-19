using Dapper;
using AP_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AP_WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AnimalController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("ListarAnimales")]
        public IActionResult ListarAnimales()
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var result = context.Query<AnimalResponse>("sp_ListarAnimales");

            if (result == null || !result.Any())
                return NotFound("No hay animales registrados");

            return Ok(result);
        }
    }
}