using System;

namespace Tcp.Core.Message
{
    public interface IMessageParser
    {
        IMessage Parse(ReadOnlySpan<byte> buffer);
    }
}