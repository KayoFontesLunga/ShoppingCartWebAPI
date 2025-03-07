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
        DataTable dataTable = new DataTable();
        try
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
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteQueryAsync: {ex.Message}");
        }
        return dataTable;
    }

    public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteNonQueryAsync: {ex.Message}");
            return -1; // Indica falha
        }
    }

    public async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null)
    {
        SqlConnection connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            // CommandBehavior.CloseConnection fecha a conexão automaticamente quando o Reader for fechado
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteReaderAsync: {ex.Message}");
            connection.Dispose(); // Fecha a conexão em caso de erro
            throw;
        }
    }

    public async Task<int> ExecuteScalarAsync(string query, SqlParameter[] parameters = null)
    {
        try
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
                    object result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteScalarAsync: {ex.Message}");
            return -1; // Indica falha
        }
    }
}
