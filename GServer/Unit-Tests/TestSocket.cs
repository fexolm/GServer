using System.Collections.Generic;
using System.Linq;
using System.Net;
using GServer;
using GServer.Containers;
using GServer.Messages;

namespace Unit_Tests
{
    internal class Datagram
    {
        public readonly byte[] Buffer;
        public readonly IPEndPoint EndPoint;

        public Datagram(byte[] buffer, IPEndPoint endPoint) {
            Buffer = buffer;
            EndPoint = endPoint;
        }
    }

    internal class TestSocket : ISocket
    {
        protected readonly Queue<Datagram> _messages = new Queue<Datagram>();
        protected readonly IDictionary<IPEndPoint, TestSocket> _sockets = new Dictionary<IPEndPoint, TestSocket>();
        protected IPEndPoint _endPoint;
        protected IPEndPoint _serverEp;

        public int Available {
            get {
                lock (_messages) {
                    return _messages.Count;
                }
            }
        }

        public void Bind(IPEndPoint localEP) {
            _endPoint = localEP;
            lock (_sockets) {
                _sockets.Add(localEP, this);
            }
        }

        public void Close() {
            lock (_sockets) {
                _sockets.Remove(_endPoint);
            }
        }

        public void Connect(IPEndPoint endPoint) {
            _serverEp = endPoint;
        }

        // ReSharper disable once RedundantAssignment    
        public byte[] Receive(ref IPEndPoint remoteEP) {
            lock (_messages) {
                var msg = _messages.Dequeue();
                remoteEP = msg.EndPoint;
                return msg.Buffer;
            }
        }

        public virtual int Send(byte[] dgram, IPEndPoint endPoint) {
            if (dgram.Length > 0) {
                var ds = DataStorage.CreateForRead(dgram);
                while (!ds.Empty) {
                    var len = ds.ReadInt32();
                    Message.Deserialize(ds.ReadBytes(len));
                }
            }
            var dm = new Datagram(dgram, _endPoint);
            _sockets[endPoint].Deliver(dm);
            return 0;
        }

        public virtual int Send(byte[] dgram) {
            if (dgram.Length > 0) {
                var ds = DataStorage.CreateForRead(dgram);
                while (!ds.Empty) {
                    var len = ds.ReadInt32();
                    Message.Deserialize(ds.ReadBytes(len));
                }
            }
            var dm = new Datagram(dgram, _endPoint);
            lock (_sockets) {
                _sockets[_serverEp].Deliver(dm);
            }
            return 0;
        }

        public void Dispose() {
            Close();
        }

        public void Deliver(Datagram dm) {
            lock (_messages) {
                _messages.Enqueue(dm);
            }
        }

        public static void Join(params TestSocket[] sockets) {
            foreach (var socket in sockets) {
                var toAdd = sockets.Where(s => s != socket);
                foreach (var element in toAdd) socket._sockets.Add(element._endPoint, element);
            }
        }
    }
}