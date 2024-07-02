using System.Net;
using ProtocolServer.Console.Server;
using ProtocolServer.Logging;
using ProtocolServer.Logging.Enums;

namespace ProtocolServer.Console;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        Logger.Log(LogType.Log, "Starting server...");
        CancellationToken token = new CancellationToken();
        using TcpServer<EchoSessionHandler> server = new(IPAddress.Any, 26950, 128);
        server.Start(token);
        Logger.Log(LogType.Log, "Server is ready!");

        await Task.Delay(Timeout.Infinite, token);
    }
}