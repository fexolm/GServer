using System;
using System.Net;

namespace GServer
{
    public interface ISocket : IDisposable
    {
        int Available { get; }
        void Bind(IPEndPoint localEP);
        void Close();
        void Connect(IPEndPoint endPoint);
        byte[] Receive(ref IPEndPoint remoteEP);
        int Send(byte[] dgram, IPEndPoint endPoint);
        int Send(byte[] dgram);
    }
}