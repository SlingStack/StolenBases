using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesService
{
	internal class ILoggerFileProvider : ILoggerProvider
	{
		private readonly string _logFilePath;

		public ILoggerFileProvider(string logFilePath)
		{
			_logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new ILoggerFile(categoryName, _logFilePath);
		}

		void IDisposable.Dispose()
		{
			//throw new NotImplementedException();
		}
	}
}
