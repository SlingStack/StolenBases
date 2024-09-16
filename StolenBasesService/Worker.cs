using StolenBasesLib;
using StolenBasesLib.Connections;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace StolenBasesService
{
    public class Worker(IBackgroundTaskQueue taskQueue, ILogger<Worker> logger) : BackgroundService
    {
        private OnBaseDB onBaseDB;
        private readonly ChannelReader<string> channel;
        private readonly CancellationTokenSource cancellationTokenSource;
  //      private readonly ILogger logger = LoggingHelper.CreateLogger("Worker");
		//private readonly ILoggerFile filelogger = LoggingHelper.CreateLogger("Worker");

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //ConversionDB.SetConnectionParameters()
                //onBaseDB.SetConnectionString("");

                //int maxIdentifier = await ConversionDB.GetMaxIdentifier<int>();
                
                //Progress<double> progress = new Progress<double>(percent => { _logger.LogInformation($"Copying Doc Handles. Percent Complete: {percent}"); });
                //await Task.Run(() => onBaseDB.RetrieveNewDocHandles(maxIdentifier, progress, 1000));
                //logger.LogInformation($"Connection Status: {ConversionDB.TestConnection()}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }


            while (!stoppingToken.IsCancellationRequested)
            {
				try
				{                    
                    IAsyncEnumerable<QueueCommand> qcEnumberable = await taskQueue.DequeueAsync(stoppingToken);
                    //List<QueueCommand> qcList = await qcEnumberable.ToListAsync();

					await foreach (QueueCommand qc in qcEnumberable)
					{
                        Task.Run(() => { qc.QueueFunc(stoppingToken, qc.Command, qc.PipeStream); }, stoppingToken);
					}

					logger.LogInformation("Looping...");
				}
				catch (OperationCanceledException)
				{
					// Prevent throwing if stoppingToken was signaled
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error occurred executing task work item.");
				}

				await Task.Delay(1000, stoppingToken);
            }
        }

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation(
				$"{nameof(Worker)} is stopping.");

			await base.StopAsync(stoppingToken);
		}
	}
}
