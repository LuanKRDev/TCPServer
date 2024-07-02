using ProtocolServer.Console.Core;
using ProtocolServer.Console.Core.Abstract;
using ProtocolServer.Console.Core.Message;
using System.Net.Sockets;

namespace ProtocolServer.Console.Server
{
    public abstract class AbstractExecutorSessionHandler : AbstractServerSessionHandler
    {
        private Task? executor;

        public override void Start(IServerSessionOwner owner, Guid key, Socket socket)
        {
            base.Start(owner, key, socket);
            executor = Task.Run(Execute);
        }

        protected abstract Task ReceivedAsync(IMessage message);

        private async Task Execute()
        {
            while (await WaitToReadAsync())
            {
                if (TryRead(out var message))
                    await ReceivedAsync(message);
            }
        }
    }
}
