using System.Diagnostics.CodeAnalysis;
using ProtocolServer.Console.Core.Message;
using ProtocolServer.Console.Shared;
using ProtocolServer.Logging;
using ProtocolServer.Logging.Enums;

namespace ProtocolServer.Console.Server
{
    public class EchoSessionHandler : AbstractExecutorSessionHandler, IMessageResolver
    {
        public bool TryGetMessageParser(int messageTypeId, [NotNullWhen(true)] out IMessageParser? parser)
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

        protected override async Task ReceivedAsync(IMessage message)
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
            Logger.Log(LogType.Log, $"Received message from client: {echoMessage.Text}");
            EchoMessage message = new EchoMessage("[Server]: Hello client!");
            await SendAsync(message);
        }
    }
}
