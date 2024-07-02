using System.Net;
using ProtocolServer.Console.Server;

namespace ProtocolServer.Console;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        CancellationToken token = new CancellationToken();
        using TcpServer<EchoSessionHandler> server = new(IPAddress.Any, 26950, 128);
        server.Start(token);

        await Task.Delay(Timeout.Infinite, token);
    }
}