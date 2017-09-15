using System.Net;

namespace Unit_Tests
{
    internal class TestSocketFixed : TestSocket
    {
        private readonly int _lossEveryNth;
        private int n;

        public TestSocketFixed(int lossEveryNth) {
            _lossEveryNth = lossEveryNth;
        }

        public override int Send(byte[] dgram) {
            n++;
            return n % _lossEveryNth != 0 ? base.Send(dgram) : 0;
        }

        public override int Send(byte[] dgram, IPEndPoint endPoint) {
            n++;
            return n % _lossEveryNth != 0 ? base.Send(dgram, endPoint) : 0;
        }
    }
}