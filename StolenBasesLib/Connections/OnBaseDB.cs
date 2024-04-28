using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRunnerLib.Connections
{
    public class OnBaseDB : SqlServer
    {
        public OnBaseDB(string connectionString) : base(connectionString) { }

        public async Task<int> RetrieveNewDocHandles(int maxDocHandleRecorded = 0, IProgress<>)
        {
            int itemsAdded = 0;

            await Connection.OpenAsync();

            string query = $"SELECT itemnum FROM hsi.itemdata WHERE itemnum > {maxDocHandleRecorded}";

            using (SqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = query;
                using(SqlDataReader reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess))
                {
                    while(await reader.ReadAsync())
                    {
                        int itemnum = await reader.GetFieldValueAsync<int>(0);
                        if(!await ConversionDB.IdentifierExists<int>(itemnum))
                        {
                            await ConversionDB.InsertConversionObject(itemnum);
                            itemsAdded++;
                        }
                    }
                }
            }

            await Connection.CloseAsync();
            return itemsAdded;
        }
    }
}
