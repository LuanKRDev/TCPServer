using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.Core.Message;
using UnityEngine;

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

        private readonly Queue<IMessage> _receiverQueue = new Queue<IMessage>();
        private readonly Queue<IFormattableMessage> _senderQueue = new Queue<IFormattableMessage>();

        private readonly object _receiverLock = new object();
        private readonly object _senderLock = new object();

        public void Dispose()
        {
            lock (_receiverLock)
            {
                _receiverQueue.Clear();
            }
            lock (_senderLock)
            {
                _senderQueue.Clear();
            }
            _socket?.Dispose();
        }

        public void Send(IFormattableMessage message)
        {
            lock (_senderLock)
            {
                _senderQueue.Enqueue(message);
            }
        }

        public async Task SendAsync(IFormattableMessage message)
        {
            lock (_senderLock)
            {
                _senderQueue.Enqueue(message);
            }
            await Task.Yield(); // Yield para garantir que a tarefa seja agendada para execução assíncrona
        }

        public abstract IMessageResolver CreateMessageResolver();

        public async ValueTask<bool> WaitToReadAsync()
        {
            await Task.Yield(); // Yield para garantir que a tarefa seja agendada para execução assíncrona
            lock (_receiverLock)
            {
                return _receiverQueue.Count > 0;
            }
        }

        public bool TryRead(out IMessage message)
        {
            lock (_receiverLock)
            {
                if (_receiverQueue.Count > 0)
                {
                    message = _receiverQueue.Dequeue();
                    return true;
                }
                else
                {
                    message = null;
                    return false;
                }
            }
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

            byte[] buffer = new byte[BufferSize];
            var resolver = CreateMessageResolver();

            while (_socket.Connected && _socket.Poll(-1, SelectMode.SelectRead))
            {
                int messageLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                
                if (messageLength >= sizeof(int) + sizeof(int))
                {
                    int messageType = BitConverter.ToInt32(buffer, 0);
                    int payloadLength = BitConverter.ToInt32(buffer, sizeof(int));

                    if (!resolver.TryGetMessageParser(messageType, out var parser))
                    {
                        MessageTypeParserNotFound(messageType);
                    }
                    else
                    {
                        var message = parser.Parse(buffer);
                        lock (_receiverLock)
                        {
                            _receiverQueue.Enqueue(message);
                        }
                    }
                }
               
            }
        }

        protected abstract void ClientTooSlow();
        protected abstract void MessageTypeParserNotFound(int messageType);

        private async Task Sender()
        {
            if (_socket == null)
                throw new InvalidOperationException();

            byte[] buffer = new byte[BufferSize];

            while (_socket.Connected && _socket.Poll(-1, SelectMode.SelectWrite))
            {
                IFormattableMessage message;
                lock (_senderLock)
                {
                    if (_senderQueue.Count > 0)
                        message = _senderQueue.Dequeue();
                    else
                        continue; // Aguarda até que haja uma mensagem para enviar
                }

                int length = message.FormatMessage(buffer);
                
                BitConverter.TryWriteBytes(buffer, message.MessageType);

                BitConverter.TryWriteBytes(buffer.AsSpan(sizeof(int)), length);

                await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, length + sizeof(int) + sizeof(int)), SocketFlags.None);
            }
        }
    }
}