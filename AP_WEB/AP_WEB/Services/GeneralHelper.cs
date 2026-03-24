using Microsoft.Data.SqlClient;
using System.Data;

namespace AP_WEB.Services
{
    public class GeneralHelper : IGeneralHelper
    {
        private readonly IConfiguration _config;
        public GeneralHelper(IConfiguration config)
        {
            _config = config;
        }
        //Para la conexión a base de datos
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
        }
    }
}
