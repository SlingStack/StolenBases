using StolenBasesService;
using StolenBasesLib.Connections;
using StolenBasesLib;

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

string? url = Environment.GetEnvironmentVariable("SUPABASE_URL");
string? key = Environment.GetEnvironmentVariable("SUPABASE_APIKEY");
if (url != null && key != null)
	await ConversionDB.SetConnectionParameters(url, key);
else
	throw new ArgumentNullException("Supabase url or api key is null");

var host = builder.Build();

MonitorLoop monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
monitorLoop.StartMonitorLoop();

host.Run();
