using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;

namespace StolenBasesService
{

	public sealed class MonitorLoop(IBackgroundTaskQueue taskQueue, ILogger<MonitorLoop> logger, IHostApplicationLifetime applicationLifetime)
	{
		private readonly CancellationToken _cancellationToken = applicationLifetime.ApplicationStopping;

		public void StartMonitorLoop()
		{
			logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");

			// Run a console user input loop in a background thread
			Task.Run(async () => await MonitorAsync());
		}

		private async ValueTask MonitorAsync()
		{
			while (!_cancellationToken.IsCancellationRequested)
			{
				NamedPipeServerStream pipeServer = new NamedPipeServerStream("StolenBasesService", PipeDirection.InOut, 2);
				int threadId = Thread.CurrentThread.ManagedThreadId;
				pipeServer.WaitForConnection();

				logger.LogInformation($"Client connected on thread {threadId}");

				StreamString ss = new StreamString(pipeServer);

				string command = ss.ReadString();
				if (!string.IsNullOrEmpty(command))
				{
					// Enqueue a background work item
					CancellationTokenSource cts = new CancellationTokenSource();
					QueueCommand qc = new(BuildWorkItemAsync, command, pipeServer);
					await taskQueue.QueueBackgroundWorkItemAsync(qc);
				}
			}
		}

		private async ValueTask BuildWorkItemAsync(CancellationToken token, string command, NamedPipeServerStream namedPipe)
		{

			if (!token.IsCancellationRequested)
			{
				try
				{
					string response = await ConsoleCommands.RunCommand(command);
					StreamString ss = new StreamString(namedPipe);
					ss.WriteString(response);
					namedPipe.Disconnect();
					namedPipe.Dispose();
				}
				catch (OperationCanceledException)
				{
					// Prevent throwing if the Delay is cancelled
				}
			}
		}
	}
}
