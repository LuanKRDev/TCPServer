using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tcp.Core;
using Tcp.Core.Abstracts;
using Tcp.Core.Message;

namespace Tcp.Server
{
    public abstract class AbstractExecutorSessionHandler : AbstractClientSessionHandler
    {
        [CanBeNull] private Task _executor;

        public override void Start(IClientSessionOwner owner, Guid key, Socket socket)
        {
            base.Start(owner, key, socket);
            _executor = Task.Run(Execute);
        }

        protected abstract Task ReceivedAsync(IMessage message);

        private async Task Execute()
        {
            while (true)
            {
                while (await WaitToReadAsync())
                {
                    if (TryRead(out var message))
                        await ReceivedAsync(message);
                }
                await Task.Delay(10);
            }
        }
    }
}