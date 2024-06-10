using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Npgsql;

namespace StolenBasesLib.Connections
{
	public class SupabaseConnection
	{
		private string url;
		private string apiKey;
		public Supabase.Client client;
		public NpgsqlConnection npgsqlConnection;

		private SupabaseConnection(string url, string apiKey, Client client)
		{
			this.url = url;
			this.apiKey = apiKey;
			this.client = client;
		}

		public static async Task<SupabaseConnection> InitializeClient(string url, string apiKey)
		{
			
			SupabaseOptions options = new SupabaseOptions
			{
				AutoConnectRealtime = true
			};

			Client client = new Client(url, apiKey, options);
			await client.InitializeAsync();

			return new SupabaseConnection(url, apiKey, client);
		}
	}
}
