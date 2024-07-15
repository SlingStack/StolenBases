using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using StolenBasesLib.Connections;
using StolenBasesLib;
using StolenBasesLib.Models.OnBase;
using StolenBasesLib.Models;
using Supabase.Postgrest.Responses;
using System.Text.Json;

namespace StolenBasesService
{
	public static class ConsoleCommands
	{
		public static async void RunCommand(string command, ILogger logger)
		{
			if (command == null) { return; }

			command = command.ToLower();
			string[] args = GetArgs(command);

			try
			{
				switch (args[0])
				{
					case "/hi":
						Hi(logger);
						break;
					case "/add":
						if (args.Length < 3)
							throw new Exception("Must provide 2 numbers.");
						Add(logger, args[1..3]);
						break;
					case "/record":
						if (args.Length < 3)
						{
							string message = "Missing argument.";
							message += "\rExpected syntax: /record [SourceName] [SourceType]";
							throw new Exception(message);
						}
							
						await Record(logger, args[1..args.Length]);
						break;
					case "/run":

						break;
					default:
						logger.LogError($"Command {args[0]} does not exist.");
						break;
				}
			}
			catch(Exception ex)
			{
				logger.LogError($"{ex.Message}");
			}
			
		}

		private static string[] GetArgs(string input)
		{
			List<string> matchList = new List<string>();
			Regex regex = new Regex("[^\\s\"']+|\"([^\"]*)\"|'([^']*)'");
			MatchCollection matches = regex.Matches(input);
			foreach (Match match in matches)
			{
				matchList.Add(match.Value);
			}

			return matchList.ToArray();
		}

		private static void Hi(ILogger logger)
		{
			logger.LogInformation("Hello.");
		}

		private static void Add(ILogger logger, string[] args)
		{
			double a, b;
			if (!double.TryParse(args[0], out a)) { logger.LogError($"{args[0]} does not parse as type double."); }
			if (!double.TryParse(args[1], out b)) { logger.LogError($"{args[1]} does not parse as type double."); }

			logger.LogInformation($"{a} + {b} = {a + b}");
		}

		private static async Task Record(ILogger logger, string[] args)
		{
			//Get OnBase connection from ConnectionManager
			Connection? connection;
			if (!ConnectionManager.TryGetConnection(args[0], out connection))
			{
				throw new ArgumentNullException($"Connection with name {args[0]} does not exist.");
			}

			if (connection == null) { throw new ArgumentNullException("Connection is null."); }

			switch (connection.ConnectionType)
			{
				case ConnectionType.OnBase:
					OnBaseDB ob = connection.Value;
					
					//Get doc handles
					if (args[0] == "all" || args[0] == "documents")
					{
						logger.LogInformation("Recording documents to conversion database.");

						//Get max doc handle
						int maxDocHandle = 0;
						var docWithMaxHandle = await ConversionDB.Connection.client
							.From<ConversionItem>()
							.Where(x => x.Source == "onbase" && x.SourceType == "document")
							.Select(x => new object[] { x.SourceId })
							.Order(x => x.SourceId, Supabase.Postgrest.Constants.Ordering.Descending)
							.Limit(1)
							.Get();
						if (docWithMaxHandle != null) { maxDocHandle = docWithMaxHandle.Models[0].SourceId; }

						//Query OnBase for new doc handles
						int[] docHandles = await ob.RetrieveNewDocHandles(maxDocHandle);
						List<ConversionItem> documentList = new List<ConversionItem>();
						foreach (int docHandle in docHandles)
						{
							documentList.Add(new ConversionItem { SourceId = docHandle, Source = "onbase", SourceType = "document" });
						}

						//Save doc handles
						await ConversionDB.Connection.client.From<ConversionItem>().Insert(documentList);
					}
					break;
				default:
					throw new NotImplementedException();
			}			
		}

		private static async Task Run()
		{
			ModeledResponse<ConversionItem> itemsToConvert = await ConversionDB.Connection.client.From<ConversionItem>()
				.Where(x => !x.WasConverted)
				.Order(x => x.SourceId, Supabase.Postgrest.Constants.Ordering.Descending)
				.Limit(1000)
				.Get();

			foreach(ConversionItem item in itemsToConvert.Models)
			{
				if(item.Source == "onbase" && item.SourceType == "document")
				{
					//Get OnBase connection from ConnectionManager
					Connection? connection;
					if (!ConnectionManager.TryGetConnection(item.SourceName, out connection))
					{
						throw new ArgumentNullException($"Connection with name {item.SourceName} does not exist.");
					}

					if (connection == null) { throw new ArgumentNullException("Connection is null."); }

					OnBaseDB ob = connection.Value;

					Document doc = await ob.GetDocumentInfo(item.SourceId);
					string json = JsonSerializer.Serialize(doc);
					ConversionDB.UpdateConversionItemData(item.ConversionId, json);
				}
			}
		}
	}
}
