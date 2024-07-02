using System;
using System.Net.Sockets;

namespace Tcp.Core
{
    public interface IClientSessionHandler : ISessionHandler
    {
        void Start(IClientSessionOwner owner, Guid id, Socket socket);
    }
}