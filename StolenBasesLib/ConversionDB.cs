using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRunnerLib
{
    public static class ConversionDB
    {
        public static Npgsql.NpgsqlConnection Connection { get; private set; } = new Npgsql.NpgsqlConnection();

        public static void SetConnectionString(string connectionString)
        {
            Connection.ConnectionString = connectionString;
        }

        public static async Task<bool> IdentifierExists<T>(T identifier)
        {
            await Connection.OpenAsync();
            string identifierStr = string.Empty;

            if(typeof(T) == typeof(string))
            {
                identifierStr = $"'{identifier}'";
            }
            else
            {
                identifierStr = identifier.ToString();
            }

            string query = $"SELECT identifier FROM ConversionObjects WHERE identifier = {identifierStr}";

            using (NpgsqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = query;
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    bool result = reader.HasRows;
                    await Connection.CloseAsync();
                    return result;
                }
            }
        }

        public static async Task InsertConversionObject<T>(T identifier)
        {
            await Connection.OpenAsync();
            string identifierStr = string.Empty;

            if (typeof(T) == typeof(string))
            {
                identifierStr = $"'{identifier}'";
            }
            else
            {
                identifierStr = identifier.ToString();
            }

            string query = $"INSERT INTO \"ConversionObjects\" (identifier, converted) VALUES ({identifierStr}, false)";

            using (NpgsqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();
                await Connection.CloseAsync();
            }
        }
        
        public static bool TestConnection()
        {
            try
            {
                Connection.Open();
                Connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<T> GetMaxIdentifier<T>()
        {
            await Connection.OpenAsync();
            string query = $"SELECT TOP (1) identifier FROM ConversionObjects ORDER BY identifier DESC";

			using (NpgsqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = query;
				using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
				{
                    if (!reader.HasRows)
                    {
						await Connection.CloseAsync();
						return default(T);
                    }

                    reader.Read();
                    T maxId = (T)reader[0];
                    await Connection.CloseAsync();
					return maxId;
				}
			}
		}

    }
}
