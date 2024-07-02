using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading.Channels;
using ProtocolServer.Console.Core.Message;

namespace ProtocolServer.Console.Core.Abstract
{
    public abstract class AbstractSessionHandler : ISessionHandler
    {
        private const int BufferSize = 1024;
        private Guid? _key;
        private Socket? _socket;
        public Guid Key => _key ?? Guid.Empty;

        private Task? _receiver;
        private Task? _sender;

        private Channel<IMessage>? _receiverChannel;
        private Channel<IFormattableMessage>? _senderChannel;

        public void Dispose()
        {
            _receiverChannel?.Writer.Complete();
            _senderChannel?.Writer.Complete();
            _socket?.Dispose();
        }

        public void Send(IFormattableMessage message)
        {
            if (!_senderChannel!.Writer.TryWrite(message))
                ClientTooSlow();
        }

        protected async Task SendAsync(IFormattableMessage message)
        {
            await _senderChannel!.Writer.WriteAsync(message);
        }

        protected abstract IMessageResolver CreateMessageResolver();

        protected ValueTask<bool> WaitToReadAsync()
        {
            return _receiverChannel!.Reader.WaitToReadAsync();
        }

        protected bool TryRead([NotNullWhen(true)] out IMessage? message)
        {
            return _receiverChannel!.Reader.TryRead(out message);
        }

        protected virtual void Start(Guid key, Socket socket)
        {
            this._key = key;
            this._socket = socket;

            _receiverChannel = Channel.CreateBounded<IMessage>(100);
            _senderChannel = Channel.CreateBounded<IFormattableMessage>(100);

            _receiver = Task.Run(Receiver);
            _sender = Task.Run(Sender);
        }

        private async Task Receiver()
        {
            if (_socket == null)
                throw new InvalidOperationException();

            Memory<byte> buffer = new byte[AbstractSessionHandler.BufferSize];
            var resolver = CreateMessageResolver();
            while (_socket.Connected && _socket.Poll(-1, SelectMode.SelectRead))
            {
                var messageLength = await _socket.ReceiveAsync(buffer);

                if (messageLength < sizeof(int) + sizeof(int)) continue;
                var messageType = BitConverter.ToInt32(buffer.Span);
                var payloadLength = BitConverter.ToInt32(buffer.Span.Slice(sizeof(int)));

                if (!resolver.TryGetMessageParser(messageType, out var parser))
                    MessageTypeParserNotFound(messageType);
                else
                {
                    var message = parser.Parse(buffer.Span.Slice(0, messageLength).Slice(sizeof(long)));
                    if (!_receiverChannel!.Writer.TryWrite(message))
                        ClientTooSlow();
                }
            }
           
        }

        protected abstract void ClientTooSlow();
        protected abstract void MessageTypeParserNotFound(int messageType);

        private async Task Sender()
        {
            if (_socket == null)
                throw new InvalidOperationException();

            Memory<byte> buffer = new byte[AbstractSessionHandler.BufferSize];
            while (await _senderChannel!.Reader.WaitToReadAsync()
                && _socket.Connected
                && _socket.Poll(-1, SelectMode.SelectWrite))
            {
                if (!_senderChannel.Reader.TryRead(out var message)) continue;
                int length = message.FormatMessage(buffer.Span.Slice(sizeof(long)));
                BitConverter.TryWriteBytes(buffer.Span, message.MessageType);
                BitConverter.TryWriteBytes(buffer.Span.Slice(sizeof(int)), length);

                await _socket.SendAsync(buffer.Slice(0, length + sizeof(int) + sizeof(int)));
            }
        }
    }
}
