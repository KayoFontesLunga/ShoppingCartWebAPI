using Microsoft.Data.SqlClient;
using System.Data;

namespace ShoppingCartWebAPI;

public class DataBaseHelper
{
    private readonly string _connectionString;
    public DataBaseHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DataBase");
    }
    public async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }
    }
    public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}

