using System;
using System.Text;
using Tcp.Core.Message;

namespace Tcp.Shared
{
    public class EchoMessage:  IFormattableMessage
    {
        private readonly string _text;

        public string Text => _text;

        public int MessageType => 1;

        public EchoMessage(string text)
        {
            this._text = text;
        }

        public int FormatMessage(Span<byte> bytes)
        {
            return Encoding.UTF8.GetBytes(_text, bytes);
        }
    }
}