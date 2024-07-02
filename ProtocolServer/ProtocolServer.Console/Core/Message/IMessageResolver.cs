using System.Diagnostics.CodeAnalysis;

namespace ProtocolServer.Console.Core.Message
{
    public interface IMessageResolver
    {
        bool TryGetMessageParser(int messageTypeId, [NotNullWhen(true)] out IMessageParser? parser);
    }
}
