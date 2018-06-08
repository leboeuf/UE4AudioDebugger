using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace UE4AudioDebugger.Server
{
    public class UdpServer
    {
        private IPEndPoint _listeningEndPoint;
        private UdpClient _udpClient;

        /// <summary>
        /// Concurrent queue where incoming message are stored for consumption by a worker thread.
        /// </summary>
        public ConcurrentQueue<Message> MessageQueue = new ConcurrentQueue<Message>();

        public UdpServer(int serverPort)
        {
            _listeningEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
           _udpClient = new UdpClient(_listeningEndPoint);
        }

        /// <summary>
        /// Start listening for incoming messages.
        /// </summary>
        public void Start()
        {
            _udpClient.BeginReceive(OnReceivedUdpMessage, _udpClient);
        }

        private void OnReceivedUdpMessage(IAsyncResult result)
        {
            var socket = result.AsyncState as UdpClient;
            var message = socket.EndReceive(result, ref _listeningEndPoint);
            MessageQueue.Enqueue(new Message
            {
                Timestamp = DateTime.Now,
                MessageBytes = message,
            });
            socket.BeginReceive(OnReceivedUdpMessage, socket);
        }
    }
}
