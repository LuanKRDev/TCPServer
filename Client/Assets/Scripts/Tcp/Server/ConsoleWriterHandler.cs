using System.Diagnostics.CodeAnalysis;
using Tcp.Core.Abstracts;
using Tcp.Core.Message;
using Tcp.Shared;

namespace Tcp.Server
{
    public class ConsoleWriterHandler : AbstractClientSessionHandler, IMessageResolver
    {

        public override IMessageResolver CreateMessageResolver()
        {
            return this;
        }

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

        protected override void ClientTooSlow()
        {
        }

        protected override void MessageTypeParserNotFound(int messageType)
        {
        }
    }
}