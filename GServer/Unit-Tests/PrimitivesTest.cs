using GServer;
using GServer.Messages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unit_Tests
{
    class PrimitivesTest
    {
        [Test]
        public void MessageCounterTest()
        {
            MessageCounter mc14 = 14;
            MessageCounter mc100 = 100;
            MessageCounter almostMax = short.MaxValue - 100;
            MessageCounter halfMax = short.MaxValue / 2 + 15;
            MessageCounter almostMin = short.MinValue + 100;
            MessageCounter mc15 = 15;
            MessageCounter max = short.MaxValue;
            MessageCounter min = short.MinValue;
            Assert.Greater(mc100, mc14, "100 < 14");
            Assert.Greater(halfMax, mc100, "halfMax < 100");
            Assert.Greater(mc14, halfMax, "14 < halfMax");
            Assert.Greater(almostMax, halfMax, "almostMax < halfMax");
            Assert.Greater(mc100, almostMax, "100 < almostMax");
            Assert.Greater(mc14, almostMax, "14 < almostMax");
            Assert.AreEqual(min - 1, 1);
            Assert.AreEqual(almostMin - 130, 30);
            Assert.AreEqual(mc100 - 100, 0);
        }

    }
}
