using System;
using System.Text;
using Tcp.Core.Message;

namespace Tcp.Shared
{
    public class EchoMessageParser: IMessageParser
    {
        public const int MessageType = 1;

        public IMessage Parse(ReadOnlySpan<byte> buffer)
        {
            return new EchoMessage(Encoding.UTF8.GetString(buffer));
        }
    }
}