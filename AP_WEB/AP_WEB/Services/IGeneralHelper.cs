using System.Data;

namespace AP_WEB.Services
{
    public interface IGeneralHelper
    {
        public IDbConnection CreateConnection();
    }
}
