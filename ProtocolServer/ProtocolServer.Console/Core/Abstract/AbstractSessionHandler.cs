using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading.Channels;
using ProtocolServer.Console.Core.Message;
using ProtocolServer.Logging;

namespace ProtocolServer.Console.Core.Abstract
{
    public abstract class AbstractSessionHandler : ISessionHandler
    {
        private const int BufferSize = 1024;
        private Guid? key;
        private Socket? socket;
        public Guid Key => key ?? Guid.Empty;

        private Task? receiver;
        private Task? sender;

        private Channel<IMessage>? receiverChannel;
        private Channel<IFormattableMessage>? senderChannel;

        public void Dispose()
        {
            receiverChannel?.Writer.Complete();
            senderChannel?.Writer.Complete();
            socket?.Dispose();
        }

        public void Send(IFormattableMessage message)
        {
            if (!senderChannel!.Writer.TryWrite(message))
                ClientTooSlow();
        }

        public async Task SendAsync(IFormattableMessage message)
        {
            await senderChannel!.Writer.WriteAsync(message);
        }

        public abstract IMessageResolver CreateMessageResolver();

        public ValueTask<bool> WaitToReadAsync()
        {
            return receiverChannel!.Reader.WaitToReadAsync();
        }

        public bool TryRead([NotNullWhen(true)] out IMessage? message)
        {
            return receiverChannel!.Reader.TryRead(out message);
        }

        protected abstract void OnDisconnect();

        public virtual void Start(Guid key, Socket socket)
        {
            this.key = key;
            this.socket = socket;

            receiverChannel = Channel.CreateBounded<IMessage>(100);
            senderChannel = Channel.CreateBounded<IFormattableMessage>(100);

            receiver = Task.Run(Receiver);
            sender = Task.Run(Sender);
        }

        private async Task Receiver()
        {
            if (socket == null)
                throw new InvalidOperationException();

            Memory<byte> buffer = new byte[AbstractSessionHandler.BufferSize];
            var resolver = CreateMessageResolver();
            while (socket.Connected && socket.Poll(-1, SelectMode.SelectRead))
            {
                var messageLength = await socket.ReceiveAsync(buffer);

                // should contain messageType and messageLength
                if (messageLength >= sizeof(int) + sizeof(int))
                {
                    var messageType = BitConverter.ToInt32(buffer.Span);
                    var payloadLength = BitConverter.ToInt32(buffer.Span.Slice(sizeof(int)));

                    if (!resolver.TryGetMessageParser(messageType, out var parser))
                        MessageTypeParserNotFound(messageType);
                    else
                    {
                        var message = parser.Parse(buffer.Span.Slice(0, messageLength).Slice(sizeof(long)));
                        if (!receiverChannel!.Writer.TryWrite(message))
                            ClientTooSlow();
                    }
                }
            }
           
        }

        protected abstract void ClientTooSlow();
        protected abstract void MessageTypeParserNotFound(int messageType);

        private async Task Sender()
        {
            if (socket == null)
                throw new InvalidOperationException();

            Memory<byte> buffer = new byte[AbstractSessionHandler.BufferSize];
            while (await senderChannel!.Reader.WaitToReadAsync()
                && socket.Connected
                && socket.Poll(-1, SelectMode.SelectWrite))
            {
                if (senderChannel.Reader.TryRead(out var message))
                {
                    int length = message.FormatMessage(buffer.Span.Slice(sizeof(long)));
                    BitConverter.TryWriteBytes(buffer.Span, message.MessageType);
                    BitConverter.TryWriteBytes(buffer.Span.Slice(sizeof(int)), length);

                    await socket.SendAsync(buffer.Slice(0, length + sizeof(int) + sizeof(int)));
                }
            }
        }
    }
}
