using BaseRunnerService;
using StolenBasesService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<MonitorLoop>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(_ =>
{
	if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
	{
		queueCapacity = 100;
	}

	return new DefaultBackgroundTaskQueue(queueCapacity);
});

var host = builder.Build();

MonitorLoop monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
monitorLoop.StartMonitorLoop();

host.Run();
