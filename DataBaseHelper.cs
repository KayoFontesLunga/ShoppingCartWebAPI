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

    // Inicia uma transação e retorna a transação para ser usada
    public async Task<SqlTransaction> BeginTransactionAsync()
    {
        SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await Task.FromResult(connection.BeginTransaction());
    }

    // Executa uma consulta e retorna um DataTable
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

    // Executa um comando SQL sem retorno (INSERT, UPDATE, DELETE) com suporte a transações
    public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            using (SqlCommand command = new SqlCommand(query, transaction?.Connection ?? new SqlConnection(_connectionString)))
            {
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                else
                {
                    await command.Connection.OpenAsync();
                }

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                int result = await command.ExecuteNonQueryAsync();

                if (transaction == null)
                {
                    await command.Connection.CloseAsync();
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteNonQueryAsync: {ex.Message}");
            return -1; // Indica falha
        }
    }

    // Executa uma consulta de leitura com suporte a transações
    public async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null, SqlTransaction transaction = null)
    {
        SqlConnection connection = transaction?.Connection ?? new SqlConnection(_connectionString);
        try
        {
            if (transaction == null)
            {
                await connection.OpenAsync();
            }

            SqlCommand command = new SqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteReaderAsync: {ex.Message}");
            connection.Dispose(); // Fecha a conexão em caso de erro
            throw;
        }
    }

    // Executa um comando SQL que retorna um valor único (SELECT COUNT(*), etc.), com suporte a transações
    public async Task<int> ExecuteScalarAsync(string query, SqlParameter[] parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            using (SqlCommand command = new SqlCommand(query, transaction?.Connection ?? new SqlConnection(_connectionString)))
            {
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                else
                {
                    await command.Connection.OpenAsync();
                }

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                object result = await command.ExecuteScalarAsync();

                if (transaction == null)
                {
                    await command.Connection.CloseAsync();
                }

                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteScalarAsync: {ex.Message}");
            return -1; // Indica falha
        }
    }
}
