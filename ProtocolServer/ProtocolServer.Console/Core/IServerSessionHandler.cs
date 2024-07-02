using System.Net.Sockets;

namespace ProtocolServer.Console.Core
{
    public interface IServerSessionHandler : ISessionHandler
    {
        void Start(IServerSessionOwner owner, Guid key, Socket socket);
    }
}
