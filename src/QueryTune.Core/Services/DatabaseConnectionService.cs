using Microsoft.Data.SqlClient;
using QueryTune.Core.Models;
using System;
using System.Threading.Tasks;

namespace QueryTune.Core.Services
{
    public class DatabaseConnectionService : IDatabaseConnectionService
    {
        public string GetConnectionString(ConnectionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return parameters.BuildConnectionString();
        }

        public async Task<bool> TestConnectionAsync(ConnectionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var connection = new SqlConnection(GetConnectionString(parameters));
            
            try
            {
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
