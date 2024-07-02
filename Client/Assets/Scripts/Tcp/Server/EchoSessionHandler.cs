using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Tcp.Core.Abstracts;
using Tcp.Core.Message;
using Tcp.Shared;
using UnityEngine;

namespace Tcp.Server
{
    public class EchoSessionHandler : AbstractClientSessionHandler, IMessageResolver
    {
        public bool TryGetMessageParser(int messageTypeId, [NotNullWhen(true)] out IMessageParser parser)
        {
            switch (messageTypeId)
            {
                case EchoMessageParser.MessageType:
                    parser = new EchoMessageParser();
                    return true;
                default:
                    parser = null;
                    return false;
            }
        }

        protected override IMessageResolver CreateMessageResolver()
        {
            return this;
        }
        protected override void ClientTooSlow()
        {

        }

        protected override void MessageTypeParserNotFound(int messageType)
        {

        }
        
        public async Task UpdateReceiver()
        {
            if (TryRead(out var message))
                await ReceivedAsync(message);
        }
        
        private  async Task ReceivedAsync(IMessage message)
        {
            switch (message)
            {
                case EchoMessage echoMessage:
                    await HandleEchoMessage(echoMessage);
                    break;
            }
        }


        private async Task HandleEchoMessage(EchoMessage echoMessage)
        {
            Debug.Log($"Received message from Server: {echoMessage.Text}");
            await Task.CompletedTask;
        }
    }
}