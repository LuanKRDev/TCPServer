using ProtocolServer.Console.Core;
using ProtocolServer.Logging;
using ProtocolServer.Logging.Enums;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ProtocolServer.Console
{
    public class TcpServer<TSessionHandler>(IPAddress address, int port, int listenBackLog)
        : IServerSessionOwner, IDisposable
        where TSessionHandler : IServerSessionHandler, new()
    {
        private readonly ConcurrentDictionary<Guid, ISessionHandler> _sessions = new();
        private readonly Socket _socket = new(SocketType.Stream, ProtocolType.Tcp);

        private Task? _acceptorTask;

        public void Start(CancellationToken token) {
            _socket.Bind(new IPEndPoint(address, port));
            _socket.Listen(listenBackLog);
            _acceptorTask = Task.Run(AcceptorCallback, token);
        }

        private async Task AcceptorCallback()
        {
            while (_acceptorTask != null && !_acceptorTask.IsCompleted) {
                Socket client = await _socket.AcceptAsync();
                Guid key = Guid.NewGuid();  
                Logger.Log(LogType.Log, $"New socket already connected with key: {key}");
                var session = new TSessionHandler();
                _sessions.TryAdd(key, session);
                session.Start(this, key, client);
            }
        }

        public void SendMessageTo(Guid key, dynamic message)
        {
            if (_sessions.TryGetValue(key, out var session)) { 
                session.Send(message);
            }
        }

        public void SendMessageToAll(dynamic message)
        {
            foreach (var session in _sessions.Values) {
                session.Send(message);
            }
        }

        public void SendMessageToAllExcept(Guid key, dynamic message)
        {
            foreach(var session in _sessions.Values) { 
                if(session.Key != key)
                {
                    session.Send(message);
                }
            }
        }

        public void DisconnectTo(Guid key)
        {
            if (_sessions.TryRemove(key, out var session)) {
                session.Dispose();
                Logger.Log(LogType.Warn, "Removed");
            }
        }

        public void Dispose()
        {
            _acceptorTask?.Dispose();
            _socket.Dispose();
        }
    }
}
