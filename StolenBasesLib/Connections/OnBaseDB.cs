using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Connections
{
    public class OnBaseDB : SqlServer
    {
        public OnBaseDB(string connectionString) : base(connectionString) { }

        public async Task<int> RetrieveNewDocHandles(int maxDocHandleRecorded, IProgress<double> progress, int reportFrequency = 1)
        {
            int itemsAdded = 0;
            int rowCount = 0;

            await Connection.OpenAsync();

            //Get row count
            using (SqlCommand command = Connection.CreateCommand())
            {
				command.CommandText = $"SELECT COUNT(1) FROM hsi.itemdata WHERE itemnum > {maxDocHandleRecorded}";
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    rowCount = (int)reader[0];
                }
			}

            using (SqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = $"SELECT itemnum FROM hsi.itemdata WHERE itemnum > {maxDocHandleRecorded}";
				using (SqlDataReader reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess))
                {
                    while(await reader.ReadAsync())
                    {
                        if (itemsAdded % reportFrequency == 0) { progress.Report((double)itemsAdded / rowCount); }
                        
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
