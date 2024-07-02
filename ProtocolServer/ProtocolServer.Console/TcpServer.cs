using ProtocolServer.Console.Core;
using ProtocolServer.Logging;
using ProtocolServer.Logging.Enums;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ProtocolServer.Console
{
    public class TcpServer<SessionHandler> : IServerSessionOwner, IDisposable where SessionHandler : IServerSessionHandler, new()
    {
        private readonly ConcurrentDictionary<Guid, ISessionHandler> sessions;
        private readonly Socket socket;

        private IPAddress address;
        private int port;
        private int listenBackLog;

        private Task? acceptorTask;

        public TcpServer(IPAddress address, int port, int listenBackLog) {
            socket = new(SocketType.Stream, ProtocolType.Tcp);
            sessions = new();
            this.address = address;
            this.port = port;
            this.listenBackLog = listenBackLog;
        }

        public void Start(CancellationToken token) {
            socket.Bind(new IPEndPoint(address, port));
            socket.Listen(listenBackLog);
            acceptorTask = Task.Run(() => AcceptorCallback(), token);
        }

        private async Task AcceptorCallback()
        {
            while (acceptorTask != null && !acceptorTask.IsCompleted) {
                Socket client = await socket.AcceptAsync();
                Guid key = Guid.NewGuid();  
                Logger.Log(LogType.Log, $"New socket already connected with key: {key}");
                var session = new SessionHandler();
                sessions.TryAdd(key, session);
                session.Start(this, key, client);
            }
        }

        public void SendMessageTo(Guid key, dynamic message)
        {
            if (sessions.TryGetValue(key, out var session)) { 
                session.Send(message);
            }
        }

        public void SendMessageToAll(dynamic message)
        {
            foreach (var session in sessions.Values) {
                session.Send(message);
            }
        }

        public void SendMessageToAllExcept(Guid key, dynamic message)
        {
            foreach(var session in sessions.Values) { 
                if(session.Key != key)
                {
                    session.Send(message);
                }
            }
        }

        public void DisconnectTo(Guid key)
        {
            if (sessions.TryRemove(key, out var session)) {
                session.Dispose();
                Logger.Log(LogType.Warn, "Removed");
            }
        }

        public void Dispose()
        {
            acceptorTask?.Dispose();
            socket.Dispose();
        }
    }
}
