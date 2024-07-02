
using ProtocolServer.Console.Core.Message;

namespace ProtocolServer.Console.Core
{
    public interface IFormattableMessage : IMessage
    {
        int FormatMessage(Span<byte> bytes);
    }
}
