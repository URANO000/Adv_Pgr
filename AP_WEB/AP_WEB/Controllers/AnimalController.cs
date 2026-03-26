using AP_WEB.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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

        [HttpGet("ListarTiposAnimal")]
        public IActionResult ListarTiposAnimal()
        {
            using var context = new SqlConnection(
                _config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var result = context.Query<AnimalTipoResponse>(
                "sp_ListarTiposAnimal",
                commandType: CommandType.StoredProcedure);

            if (result == null || !result.Any())
                return NotFound("No hay tipos");

            return Ok(result);
        }
    }
}