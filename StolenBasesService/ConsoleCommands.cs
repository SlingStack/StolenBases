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
using Supabase.Core;
using System.Web;
using Azure;

namespace StolenBasesService
{
	public static class ConsoleCommands
	{
		private static CancellationTokenSource cancellationTokenSource = new();
		private static string loggerCategory = "ConsoleCommands";

		public static async Task<string> RunCommand(string command)
		{
			string response = "";

			if (command == null) { return ""; }
			if (!LoggingHelper.Exists(loggerCategory))
			{
				LoggingHelper.AddLogger(loggerCategory, LoggingType.All);
			}


			command = command.ToLower();
			string[] args = GetArgs(command);

			try
			{
				switch (args[0])
				{
					case "/hi":
						response = Hi();
						break;
					case "/add":
						if (args.Length < 3)
							throw new Exception("Must provide 2 numbers.");
						response = Add(args[1..3]);
						break;
					case "/record":
						if (args.Length < 3)
						{
							string message = "Missing argument.";
							message += "\rExpected syntax: /record [SourceName] [SourceType]";
							throw new Exception(message);
						}

						await Record(args[1..args.Length]);
						break;
					case "/run":
						Run();
						break;
					case "/runtest":
						response = "Running Test...";
						Task.Run(RunTest);
						break;
					case "/stop":
						await cancellationTokenSource.CancelAsync();
						cancellationTokenSource = new CancellationTokenSource();
						break;
					default:
						Log($"Command {args[0]} does not exist.", LogLevel.Error);
						break;
				}
			}
			catch (Exception ex)
			{
				Log($"{ex.Message}", LogLevel.Error);
			}

			return response;

		}

		private static void Log(string message)
		{
			LoggingHelper.Log(loggerCategory, message);
		}

		private static void Log(string message, LogLevel logLevel)
		{
			LoggingHelper.Log(loggerCategory, message, logLevel);
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

		private static string Hi()
		{
			Log("Hello.");
			return "Hello.";
		}

		private static string Add(string[] args)
		{
			double a, b;
			if (!double.TryParse(args[0], out a)) { Log($"{args[0]} does not parse as type double.", LogLevel.Error); }
			if (!double.TryParse(args[1], out b)) { Log($"{args[1]} does not parse as type double.", LogLevel.Error); }

			Log($"{a} + {b} = {a + b}");
			return (a + b).ToString();
		}

		private static async Task Record(string[] args)
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
						Log("Recording documents to conversion database.");

						//Get max doc handle
						int maxDocHandle = 0;
						var docWithMaxHandle = await ConversionDB.Connection.client
							.From<ConversionItem>()
							.Where(x => x.SourcePlatform == "onbase" && x.SourceType == "document")
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
							documentList.Add(new ConversionItem { SourceId = docHandle, SourcePlatform = "onbase", SourceType = "document" });
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
			CancellationToken cancellationToken = cancellationTokenSource.Token;

			while (!cancellationToken.IsCancellationRequested)
			{
				ModeledResponse<ConversionItem> itemsToConvert = await ConversionDB.Connection.client.From<ConversionItem>()
				.Where(x => !x.WasConverted)
				.Order(x => x.SourceId, Supabase.Postgrest.Constants.Ordering.Descending)
				.Limit(1000)
				.Get();

				foreach (ConversionItem item in itemsToConvert.Models)
				{
					if (item.SourcePlatform == "onbase" && item.SourceType == "document")
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

					if (cancellationToken.IsCancellationRequested) { break; }
				}
			}
		}

		private static async Task RunTest()
		{
			CancellationToken cancellationToken = cancellationTokenSource.Token;

			int i = 0;
			while (!cancellationToken.IsCancellationRequested && i < 5)
			{
				Log((i+1).ToString());
				i++;
				Thread.SpinWait(50000000);
			}
		}
	}
}
