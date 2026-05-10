// Services/BaseService.cs
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace NexumApp.Services
{
    public class BaseService
    {
        protected MySqlConnection GetConnection()
        {
            return DatabaseConfig.GetConnection();
        }

        protected async Task<DataTable> ExecuteQueryAsync(string query)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                using (var adapter = new MySqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        protected async Task<int> ExecuteNonQueryAsync(string query, MySqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}