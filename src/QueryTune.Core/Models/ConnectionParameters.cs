namespace QueryTune.Core.Models
{
    public class ConnectionParameters
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public bool UseWindowsAuthentication { get; set; } = true;
        public string UserId { get; set; }
        public string Password { get; set; }

        public string BuildConnectionString()
        {            
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = ServerName,
                InitialCatalog = DatabaseName,
                IntegratedSecurity = UseWindowsAuthentication,
                TrustServerCertificate = true
            };

            if (!UseWindowsAuthentication)
            {
                builder.UserID = UserId;
                builder.Password = Password;
            }

            return builder.ConnectionString;
        }
    }
}
