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

        public static async Task<bool> IdentifierExists<T>(T identifier, Type table)
        {
            if(Connection == null)
                throw new ArgumentNullException(nameof(Connection));

            switch (table.ToString())
            {
                case "OnBaseDocument":
					var result = await Connection.client.From<Document>().Where(a => a.DocHandle == Convert.ToInt32(identifier)).Single();
                    return result == null ? false : true;
                default:
                    throw new NotSupportedException();
			}
        }

        public static async Task InsertConversionObject<T>(T identifier, Type table)
        {
			if (Connection == null)
				throw new ArgumentNullException(nameof(Connection));

			switch (table.ToString())
			{
				case "OnBaseDocument":
                    Document document = new Document { DocHandle = Convert.ToInt32(identifier) };
					var result = await Connection.client.From<Document>().Insert(document);
                    return;
				default:
					throw new NotSupportedException();
			}
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

        public static async Task<T> GetMaxIdentifier<T>(Type table)
        {
			if (Connection == null)
				throw new ArgumentNullException(nameof(Connection));

			switch (table.ToString())
			{
				case "OnBaseDocument":
					var result = await Connection.client.From<Document>().Order(a => a.DocHandle, Constants.Ordering.Descending).Single();
                    return result == null ? (T)Convert.ChangeType(-1, typeof(T)) : (T) Convert.ChangeType(result.DocHandle, typeof(T));
				default:
					throw new NotSupportedException();
			}
		}

    }
}
