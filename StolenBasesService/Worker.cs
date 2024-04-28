using BaseRunnerLib;
using BaseRunnerLib.Connections;
using Microsoft.Extensions.Logging;

namespace BaseRunnerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private OnBaseDB onBaseDB;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                ConversionDB.SetConnectionString("Host=127.0.0.1;port=5432;Database=postgres;Username=postgres;Password=password");
                onBaseDB.SetConnectionString("");
                await onBaseDB.RetrieveNewDocHandles();
                _logger.LogInformation($"Connection Status: {ConversionDB.TestConnection()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
