using GServer;
using System;
using System.Collections.Generic;
using System.Net;

namespace Unit_Tests
{
    class Datagram
    {
        public readonly byte[] Buffer;
        public readonly IPEndPoint EndPoint;
        public Datagram(byte[] buffer, IPEndPoint endPoint)
        {
            Buffer = buffer;
            EndPoint = endPoint;
        }
    }

    class TestSocket : ISocket
    {
        protected IPEndPoint _endPoint;
        protected IPEndPoint _serverEp = null;
        protected readonly Queue<Datagram> _messages = new Queue<Datagram>();
        protected readonly static IDictionary<IPEndPoint, TestSocket> _sockets = new Dictionary<IPEndPoint, TestSocket>();
        public int Available
        {
            get
            {
                lock (_messages)
                {
                    return _messages.Count;
                }
            }
        }

        public void Bind(IPEndPoint localEP)
        {
            _endPoint = localEP;
            lock (_sockets)
            {
                _sockets.Add(localEP, this);
            }
        }

        public void Close()
        {
            lock (_sockets)
            {
                _sockets.Remove(_endPoint);
            }
        }

        public void Connect(IPEndPoint endPoint)
        {
            _serverEp = endPoint;
        }

        public byte[] Receive(ref IPEndPoint remoteEP)
        {
            lock (_messages)
            {
                var msg = _messages.Dequeue();
                remoteEP = msg.EndPoint;
                return msg.Buffer;
            }
        }

        public virtual int Send(byte[] dgram, IPEndPoint endPoint)
        {
            var dm = new Datagram(dgram, _endPoint);
            lock (_sockets)
            {
                _sockets[endPoint].Deliver(dm);
            }
            return 0;
        }

        public virtual int Send(byte[] dgram)
        {
            var dm = new Datagram(dgram, _endPoint);
            lock (_sockets)
            {
                _sockets[_serverEp].Deliver(dm);
            }
            return 0;
        }
        public void Deliver(Datagram dm)
        {
            lock (_messages)
            {
                _messages.Enqueue(dm);
            }
        }
    }

    class TestSocketRnd : TestSocket
    {
        private readonly double _lossRate;
        private readonly Random rnd = new Random();
        public TestSocketRnd(double lr)
        {
            _lossRate = lr;
        }
        public TestSocketRnd()
        {
            _lossRate = 1D;
        }
        public override int Send(byte[] dgram, IPEndPoint endPoint)
        {
            if (rnd.NextDouble() < _lossRate)
            {
                return base.Send(dgram, endPoint);
            }
            return 0;
        }

        public override int Send(byte[] dgram)
        {
            if (rnd.NextDouble() < _lossRate)
            {
                return base.Send(dgram);
            }
            return 0;
        }
    }
    class TestSocketFixed : TestSocket
    {
        private readonly int _lossEveryNth;
        private int n = 0;
        public TestSocketFixed(int lossEveryNth)
        {
            _lossEveryNth = lossEveryNth;
        }
        public override int Send(byte[] dgram)
        {
            n++;
            if (n % _lossEveryNth != 0)
                return base.Send(dgram);
            return 0;
        }
        public override int Send(byte[] dgram, IPEndPoint endPoint)
        {
            n++;
            if (n % _lossEveryNth != 0)
                return base.Send(dgram, endPoint);
            return 0;
        }
    }
}
