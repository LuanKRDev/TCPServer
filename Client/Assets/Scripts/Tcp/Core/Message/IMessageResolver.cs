using System.Diagnostics.CodeAnalysis;

namespace Tcp.Core.Message
{
    public interface IMessageResolver
    {
        bool TryGetMessageParser(int messageTypeId, [NotNullWhen(true)] out  IMessageParser parser);
    }
}