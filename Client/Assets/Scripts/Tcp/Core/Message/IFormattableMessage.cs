using System;

namespace Tcp.Core.Message
{
    public interface IFormattableMessage : IMessage
    {
        int FormatMessage(Span<byte> bytes);
    }
}