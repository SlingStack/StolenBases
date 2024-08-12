using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesService
{
	internal class ILoggerFileProvider : ILoggerProvider
	{
		private readonly StreamWriter _logFileWriter;

		public ILoggerFileProvider(StreamWriter logFileWriter)
		{
			_logFileWriter = logFileWriter ?? throw new ArgumentNullException(nameof(logFileWriter));
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new ILoggerFile(categoryName, _logFileWriter);
		}

		public void Dispose()
		{
			_logFileWriter.Dispose();
		}
	}
}
