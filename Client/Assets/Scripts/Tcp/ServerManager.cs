using System;
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

            _timer = new Timer(_ =>
            {
                _client.Session.Send(new EchoMessage("[Client]: Hello server!"));
            }, null, 1000, 10);

        }

        private async void Update()
        {
            if (_client.Session != null && _client.Session.IsConnected())
            {
                await _client.Session.UpdateReceiver();
            }
        }

        private void OnDestroy()
        {
            _timer.Dispose();
            _client.Dispose();
        }
    }
}