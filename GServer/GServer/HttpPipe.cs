using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace GServer
{
    internal class HttpPipe
    {
        private TcpClient _listener;
        private bool _isListening;
        public HttpPipe(int port)
        {
            var ip = new IPEndPoint(IPAddress.Any, port);
            _listener = new TcpClient(ip);
            Thread _listenThread = new Thread(() =>
            {
                while (_isListening)
                {
                    var stream = _listener.GetStream();
                    while (!stream.DataAvailable) ;
                    byte[] bytes = new byte[_listener.Available];
                    stream.Read(bytes, 0, bytes.Length);
                    if (DataRecieved != null)
                    {
                        DataRecieved.Invoke(bytes);
                    }
                }
            });
        }
        public void Send(byte[] buffer)
        {
            _listener.GetStream().Write(buffer, 0, buffer.Length);
        }
        public void Connect(IPEndPoint masterEndPoint)
        {
            _listener.Connect(masterEndPoint);
        }
        public void Listen()
        {
            _isListening = true;
        }
        public void StopListen()
        {
            _isListening = false;
        }
        public event Action<byte[]> DataRecieved;
    }
}
