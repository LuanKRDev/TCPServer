using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.Core.Message;

namespace Tcp.Core.Abstracts
{
    public abstract class AbstractSessionHandler : ISessionHandler
    {
        private const int BufferSize = 1024;
        private Guid? _key;
        private Socket _socket;
        public Guid Key => _key ?? Guid.Empty;

        private Task _receiver;
        private Task _sender;

        private readonly BlockingCollection<IMessage> _receiverQueue = new BlockingCollection<IMessage>();
        private readonly BlockingCollection<IFormattableMessage> _senderQueue = new BlockingCollection<IFormattableMessage>();

        public void Dispose()
        {
            _receiverQueue.Dispose();
            _senderQueue.Dispose();
            _socket?.Dispose();
        }

        public void Send(IFormattableMessage message)
        {
            _senderQueue.Add(message);
        }

        public async Task SendAsync(IFormattableMessage message)
        {
            _senderQueue.Add(message);
            await Task.Yield(); 
        }

        protected abstract IMessageResolver CreateMessageResolver();

        protected async ValueTask<bool> WaitToReadAsync()
        {
            await Task.Yield(); // Garante que a tarefa seja agendada de forma assíncrona
            return _receiverQueue.Count > 0;
        }

        protected bool TryRead(out IMessage message)
        {
            return _receiverQueue.TryTake(out message);
        }

        protected void Start(Guid key, Socket socket)
        {
            _key = key;
            _socket = socket;

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
                var messageLength = await _socket.ReceiveAsync(buffer,  SocketFlags.None);

                if (messageLength < sizeof(int) + sizeof(int)) continue;
                var messageType = BitConverter.ToInt32(buffer.Span);
                var payloadLength = BitConverter.ToInt32(buffer.Span.Slice(sizeof(int)));

                if (!resolver.TryGetMessageParser(messageType, out var parser))
                    MessageTypeParserNotFound(messageType);
                else
                {
                    var message = parser.Parse(buffer.Span.Slice(0, messageLength).Slice(sizeof(long)));
                    _receiverQueue.Add(message);
                }
            }
           
        }

        protected abstract void ClientTooSlow();
        protected abstract void MessageTypeParserNotFound(int messageType);

        private async Task Sender()
        {
            if (_socket == null)
                throw new InvalidOperationException();

            byte[] buffer = new byte[AbstractSessionHandler.BufferSize];

            while (_socket.Connected && _socket.Poll(-1, SelectMode.SelectWrite))
            {
                IFormattableMessage message;
                if (_senderQueue.TryTake(out message, Timeout.Infinite))
                {
                    int length = message.FormatMessage(buffer.AsSpan(sizeof(long)));

                    BitConverter.TryWriteBytes(new Span<byte>(buffer, 0, sizeof(int)), message.MessageType);

                    BitConverter.TryWriteBytes(new Span<byte>(buffer, sizeof(int), sizeof(int)), length);

                    await SendAsync(buffer, 0, length + sizeof(int) + sizeof(int));
                }
            }
        }

        private Task<int> SendAsync(byte[] buffer, int offset, int size)
        {
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, offset, size);

            var tcs = new TaskCompletionSource<int>();
            args.Completed += (_, eventArgs) =>
            {
                if (eventArgs.SocketError != SocketError.Success)
                    tcs.TrySetException(new SocketException((int)eventArgs.SocketError));
                else
                    tcs.TrySetResult(eventArgs.BytesTransferred);
            };

            if (!_socket.SendAsync(args))
            {
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                tcs.SetResult(args.BytesTransferred);
            }

            return tcs.Task;
        }

        public bool IsConnected() => _socket.Connected;
    }
}