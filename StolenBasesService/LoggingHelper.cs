using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace StolenBasesService
{
	internal static class LoggingHelper
	{
		internal static ILoggerFactory LoggerFactoryConsole { get; set; }
		internal static ILoggerFactory LoggerFactoryFile { get; set; }

		internal static ILogger CreateConsoleLogger<T>() => LoggerFactoryConsole.CreateLogger<T>();
		internal static ILogger CreateConsoleLogger(string categoryName) => LoggerFactoryConsole.CreateLogger(categoryName);

		internal static ILogger CreateFileLogger<T>() => LoggerFactoryFile.CreateLogger<T>();
		internal static ILogger CreateFileLogger(string categoryName) => LoggerFactoryFile.CreateLogger(categoryName);

		static LoggingHelper()
		{
			LoggerFactoryConsole = new LoggerFactory();
			//LoggerFactoryConsole

			LoggerFactoryFile = new LoggerFactory();
			LoggerFactoryFile.AddProvider(new ILoggerFileProvider(new StreamWriter("log.txt")));
		}
	}
}
