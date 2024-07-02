using System.Net;
using System.Threading;
using Tcp.Server;
using Tcp.Shared;
using UnityEngine;

namespace Tcp
{
    public class ServerManager : MonoBehaviour
    {
        private readonly TcpClient<EchoSessionHandler> _client = new TcpClient<EchoSessionHandler>(IPAddress.Parse("127.0.0.1"), 26950);
        private Timer _timer;

        public async void Start()
        {
            await _client.ConnectAsync();

            // Inicia o timer para enviar mensagens a cada 10ms após 1 segundo
            _timer = new Timer(_ =>
            {
                _client.Session.Send(new EchoMessage("Hello!"));
            }, null, 1000, 10);

        }
        private void OnDestroy()
        {
            _timer.Dispose();
            _client.Dispose();
        }
    }
}