using BaseRunnerLib;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

ConversionDB.SetConnectionString("Host=host.docker.internal;port=5432;Database=BaseRunnerDB;Username=postgres;Password=password");
ConversionDB.TestConnection();