using StolenBasesService;
using StolenBasesLib.Connections;
using StolenBasesLib;
using Microsoft.Extensions.Logging;


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

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();
builder.Logging.AddProvider(new ILoggerFileProvider("log.txt"));


var host = builder.Build();
var config = host.Services.GetRequiredService<IConfiguration>();

LogLevel defaultLogLevel;
if(Enum.TryParse<LogLevel>(config.GetValue<string>("Logging:LogLevel:Default"), out defaultLogLevel))
{
	LoggingHelper.defaultLogLevel = defaultLogLevel;
}

string? url = config.GetValue<string>("Supabase:URL");
string? key = config.GetValue<string>("Supabase:APIKey"); ;
//if (url != null && key != null)
//	await ConversionDB.SetConnectionParameters(url, key);
//else
//	throw new ArgumentNullException("Supabase url or api key is null");

string? obConnString = config.GetValue<string>("OnBase:ConnectionString");
if (obConnString == null)
	throw new ArgumentNullException("OnBase connection string is missing.");

OnBaseDB onBaseDB = new OnBaseDB(obConnString);
Connection obConnection = new Connection("OB1", onBaseDB, ConnectionType.OnBase, true);

MonitorLoop monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
monitorLoop.StartMonitorLoop();

host.Run();
