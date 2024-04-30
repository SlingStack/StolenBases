using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace StolenBasesService
{
	public static class ConsoleCommands
	{
		public static void RunCommand(string command, ILogger logger)
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
	}
}
