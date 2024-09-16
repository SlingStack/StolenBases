using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesService
{
	internal class ILoggerFile : ILogger
	{
		private readonly string _categoryName;
		private readonly string _logFilePath;

		public ILoggerFile(string categoryName, string logFilePath)
		{
			_categoryName = categoryName;
			_logFilePath = logFilePath;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			// Ensure that only information level and higher logs are recorded
			return logLevel >= LogLevel.Trace;
		}

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			// Ensure that only information level and higher logs are recorded
			if (!IsEnabled(logLevel))
			{
				return;
			}

			// Get the formatted log message
			var message = formatter(state, exception);

			//Write log messages to text file
			using(StreamWriter sw = new(_logFilePath))
			{
				sw.WriteLine($"[{logLevel}] [{_categoryName}] {message}");
				sw.Flush();
			}	
			
		}
	}
}
