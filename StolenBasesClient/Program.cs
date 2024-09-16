// See https://aka.ms/new-console-template for more information


using StolenBasesClient;
using System.IO.Pipes;
using System.Security.Principal;

string command = "";

while(command != "exit")
{
	command = Console.ReadLine();
	NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "StolenBasesService", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
	Console.WriteLine("Connecting to service...");
	pipeClient.Connect();
	Console.WriteLine("Connected.");
	StreamString ss = new StreamString(pipeClient);
	ss.WriteString(command);
	Console.WriteLine(ss.ReadString());

	pipeClient.Close();
	pipeClient.Dispose();
}
