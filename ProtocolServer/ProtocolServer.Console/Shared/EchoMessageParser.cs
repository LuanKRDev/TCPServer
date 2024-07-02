using ProtocolServer.Console.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolServer.Console.Shared
{
    internal class EchoMessageParser : IMessageParser
    {
        public const int MessageType = 1;

        public IMessage Parse(ReadOnlySpan<byte> buffer)
        {
            return new EchoMessage(Encoding.UTF8.GetString(buffer));
        }
    }
}
