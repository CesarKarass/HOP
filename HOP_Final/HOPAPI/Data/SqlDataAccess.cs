using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace HOPAPI.Data // <--- AÑADE ESTO
{
    public class SqlDataAccess
    {
        private readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        public async Task ExecuteNonQueryAsync(string spName, SqlParameter[] parameters)
        {
            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            
            if (parameters != null) 
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> ExecuteQueryAsync(string spName, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            
            return dt;
        }
    }
}