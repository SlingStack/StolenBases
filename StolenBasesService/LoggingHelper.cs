using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace StolenBasesService
{
	public enum LoggingType
	{
		Console,
		File,
		ClientResponse,
		All
	}
	
	internal static class LoggingHelper
	{
		internal static Dictionary<string, List<ILogger>> Loggers { get; } = new Dictionary<string, List<ILogger>>();
		internal static LogLevel defaultLogLevel { get; set; } = LogLevel.None;

		internal static ILoggerFactory LoggerFactoryConsole { get; set; }
		internal static ILoggerFactory LoggerFactoryFile { get; set; }

		internal static ILogger CreateConsoleLogger(string categoryName) => LoggerFactoryConsole.CreateLogger(categoryName);

		internal static ILogger CreateFileLogger(string categoryName) => LoggerFactoryFile.CreateLogger(categoryName);

		static LoggingHelper()
		{
			LoggerFactoryConsole = LoggerFactory.Create(a => { a.AddConsole(); a.SetMinimumLevel(LogLevel.Trace); });
			LoggerFactoryFile = new LoggerFactory();
			LoggerFactoryFile.AddProvider(new ILoggerFileProvider("log.txt"));
		}

		internal static void Log(string categoryName, string message, LogLevel logLevel)
		{
			List<ILogger>? loggers;

			if (Loggers.TryGetValue(categoryName, out loggers))
			{
				foreach (ILogger logger in loggers)
				{
					logger.Log(logLevel, message);
				}
			}
		}

		internal static void Log(string categoryName, string message)
		{
			Log(categoryName, message, defaultLogLevel);
		}

		internal static void Log(string[] categoryNames, string message, LogLevel logLevel)
		{
			foreach(string categoryName in categoryNames)
			{
				List<ILogger>? loggers;

				if (Loggers.TryGetValue(categoryName, out loggers))
				{
					foreach (ILogger logger in loggers)
					{
						logger.Log(logLevel, message);
					}
				}
			}
		}

		internal static void Log(string[] categoryNames, string message)
		{
			Log(categoryNames, message, defaultLogLevel);
		}

		internal static void AddLogger(string categoryName, LoggingType type)
		{
			List<ILogger> loggers = CreateLogger(categoryName, type);

			if (loggers.Count == 0)
				return;

			if (!Loggers.ContainsKey(categoryName))
			{
				Loggers.Add(categoryName, new List<ILogger>());
				Loggers[categoryName].AddRange(loggers);
			}
			else
			{
				Loggers[categoryName].AddRange(loggers);
			}
		}

		private static List<ILogger> CreateLogger(string categoryName, LoggingType type)
		{
			List<ILogger> loggers = new List<ILogger>();

			switch (type)
			{
				case LoggingType.Console:
					loggers.Add(CreateConsoleLogger(categoryName));
					break;
				case LoggingType.File:
					loggers.Add(CreateFileLogger(categoryName));
					break;
				case LoggingType.All:
					loggers.Add(CreateConsoleLogger(categoryName));
					loggers.Add(CreateFileLogger(categoryName));
					break;
				default:
					throw new NotImplementedException("Logger type not supported.");
			}

			return loggers;
		}

		internal static bool Exists(string categoryName)
		{
			return Loggers.ContainsKey(categoryName);
		}
	}
}
