using System;
using System.Net;
using GServer.Containers;
using GServer.Messages;

namespace Unit_Tests
{
    internal class TestSocketRnd : TestSocket
    {
        private readonly double _lossRate;
        private readonly Random rnd = new Random();

        public TestSocketRnd(double lr) {
            _lossRate = lr;
        }

        public TestSocketRnd() {
            _lossRate = 1D;
        }

        public override int Send(byte[] dgram, IPEndPoint endPoint) {
            if (rnd.NextDouble() < _lossRate) {
                return base.Send(dgram, endPoint);
            }
            if (dgram.Length <= 0) return 0;
            var ds = DataStorage.CreateForRead(dgram);
            while (!ds.Empty) {
                var len = ds.ReadInt32();
                Message.Deserialize(ds.ReadBytes(len));
            }
            return 0;
        }

        public override int Send(byte[] dgram) {
            if (rnd.NextDouble() < _lossRate) {
                return base.Send(dgram);
            }
            if (dgram.Length <= 0) return 0;
            var ds = DataStorage.CreateForRead(dgram);
            while (!ds.Empty) {
                var len = ds.ReadInt32();
                Message.Deserialize(ds.ReadBytes(len));
            }
            return 0;
        }
    }
}