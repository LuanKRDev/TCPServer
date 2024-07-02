using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tcp.Server;
using Tcp.Shared;
using UnityEngine;

namespace Tcp
{
    public class ServerManager : MonoBehaviour
    {
        private readonly TcpClient<ConsoleWriterHandler> _client = new TcpClient<ConsoleWriterHandler>(IPAddress.Parse("127.0.0.1"), 26950);

        public async void Start()
        {
            await _client.ConnectAsync();
            await Task.Run(RunTest);
        }

        private async Task RunTest()
        {
            Timer timer = new Timer(o =>
            {
                _client.Session.Send(new EchoMessage("Hello!"));
            }, null, 1000, 10);
            
            while (await _client.Session.WaitToReadAsync())
            {
                Debug.Log("Inside WaitToRead RunTest");

                if (_client.Session.TryRead(out var message))
                {
                    Debug.Log("Inside TryRead RunTest");

                    switch (message)
                    {
                        case EchoMessage echoMessage:
                            Debug.Log($"Received message EchoMessage {{ Text={echoMessage.Text} }}");
                            break;
                        default:
                            break;
                    }
                }
            }
            Debug.Log("End RunTest");
        }
    }
}