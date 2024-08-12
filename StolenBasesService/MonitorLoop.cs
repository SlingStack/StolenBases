using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				string command = Console.ReadLine();
				if (!string.IsNullOrEmpty(command))
				{
					// Enqueue a background work item
					CancellationTokenSource cts = new CancellationTokenSource();
					QueueCommand qc = new(BuildWorkItemAsync, command);
					await taskQueue.QueueBackgroundWorkItemAsync(qc);
				}
			}
		}

		private async ValueTask BuildWorkItemAsync(CancellationToken token, string command)
		{

			if (!token.IsCancellationRequested)
			{
				try
				{
					ConsoleCommands.RunCommand(command);
				}
				catch (OperationCanceledException)
				{
					// Prevent throwing if the Delay is cancelled
				}
			}
		}
	}
}
