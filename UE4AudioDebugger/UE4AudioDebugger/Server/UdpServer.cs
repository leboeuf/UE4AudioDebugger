using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UE4AudioDebugger.Server
{
    public class UdpServer
    {
        private IPEndPoint _listeningEndPoint;
        private UdpClient _udpClient;
        private ConcurrentQueue<byte[]> _receivedPacketQueue = new ConcurrentQueue<byte[]>();

        public UdpServer(int serverPort)
        {
            _listeningEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
           _udpClient = new UdpClient(_listeningEndPoint);
        }

        public void Start()
        {
            _udpClient.BeginReceive(OnReceivedUdpMessage, _udpClient);
        }

        public void ProcessQueue()
        {
            while (true)
            {
                var hasMessage = _receivedPacketQueue.TryDequeue(out byte[] message);
                if (!hasMessage)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                var data = Encoding.ASCII.GetString(message).TrimEnd('\0').Substring(4); // Substring to workaround UE4 "Writer << message" issue
                Console.WriteLine(data);
            }
        }

        private void OnReceivedUdpMessage(IAsyncResult result)
        {
            var socket = result.AsyncState as UdpClient;
            var message = socket.EndReceive(result, ref _listeningEndPoint);
            _receivedPacketQueue.Enqueue(message);
            socket.BeginReceive(OnReceivedUdpMessage, socket);
        }
    }
}
