using StolenBasesService;
using StolenBasesLib.Connections;
using StolenBasesLib;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
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
var config = host.Services.GetRequiredService<IConfiguration>();

string? url = config.GetValue<string>("Supabase:URL");
string? key = config.GetValue<string>("Supabase:APIKey"); ;
if (url != null && key != null)
	await ConversionDB.SetConnectionParameters(url, key);
else
	throw new ArgumentNullException("Supabase url or api key is null");



MonitorLoop monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
monitorLoop.StartMonitorLoop();

host.Run();
