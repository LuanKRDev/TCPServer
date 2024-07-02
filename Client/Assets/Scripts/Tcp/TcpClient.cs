using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.Core;

namespace Tcp
{
    public class TcpClient<TSessionHandler> : IDisposable, IClientSessionOwner 
        where TSessionHandler : IClientSessionHandler, new()
    {
        private readonly Socket _socket;
        private readonly IPAddress _address;
        private readonly int _port;
        
        private TSessionHandler _session;

        public TSessionHandler Session => _session;

        public TcpClient(IPAddress address, int port)
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this._address = address;
            this._port = port;
        }
        
        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

        public async Task ConnectAsync()
        {
            await _socket.ConnectAsync(_address, _port);
            _session = new TSessionHandler();
            _session.Start(this, Guid.Empty, _socket);
        }

        public void Disconnected(Guid key)
        {
            
        }
    }
}