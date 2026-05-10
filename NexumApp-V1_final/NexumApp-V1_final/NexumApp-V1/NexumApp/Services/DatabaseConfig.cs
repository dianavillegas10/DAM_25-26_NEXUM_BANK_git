// Services/DatabaseConfig.cs
using System.Configuration;
using MySql.Data.MySqlClient;

namespace NexumApp.Services
{
    public static class DatabaseConfig
    {
        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["NexumDB"]?.ConnectionString
                    ?? "Server=mysql-nexum.alwaysdata.net;Port=3306;Database=nexum_db;Uid=nexum;Pwd=LmSvJk_STz28mpL;SslMode=Required;";
            }
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}