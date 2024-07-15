using Microsoft.Data.SqlClient;
using Npgsql;
using StolenBasesLib.Connections;
using StolenBasesLib.Models;
using StolenBasesLib.Models.OnBase;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib
{
    public static class ConversionDB
    {
        public static SupabaseConnection? Connection { get; private set; }

        public static async Task SetConnectionParameters(string url, string apiKey)
        {
            Connection = await SupabaseConnection.InitializeClient(url, apiKey);
        }

        public static async Task<bool> IdentifierExists(int identifier, string sourceName)
        {
            if(Connection == null)
                throw new ArgumentNullException(nameof(Connection));


			var result = await Connection.client.From<ConversionItem>().Where(a => a.SourceId == Convert.ToInt32(identifier)).Single();
            return result == null ? false : true;
        }

        public static async Task InsertConversionObject(int identifier, ConversionItem item)
        {
			if (Connection == null)
				throw new ArgumentNullException(nameof(Connection));

			var result = await Connection.client.From<ConversionItem>().Insert(item);
			return;
		}
        
        //public static bool TestConnection()
        //{
        //    try
        //    {
        //        Client.Open();
        //        Client.Close();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static async Task<int> GetMaxIdentifier(string sourceName, string sourceType)
        {
			if (Connection == null)
				throw new ArgumentNullException(nameof(Connection));

			var result = await Connection.client.From<ConversionItem>()
                .Where(a => a.SourceName == sourceName && a.SourceType == sourceType)
                .Order(a => a.SourceId, Constants.Ordering.Descending).Single();
			return result == null ? -1 : result.SourceId;
		}

        public static async Task<bool> UpdateConversionItemData(int conversionId, string jsonData)
        {
			if (Connection == null)
				throw new ArgumentNullException(nameof(Connection));

            var model = await Connection.client.From<ConversionItem>()
                .Where(a => a.ConversionId == conversionId)
                .Single();

            model.Data = jsonData;

            var result = await model.Update<ConversionItem>();

            if (result.ResponseMessage.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
