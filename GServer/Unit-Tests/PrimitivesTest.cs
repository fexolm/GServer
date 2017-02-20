using GServer;
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
            MessageCounter mc14 = new MessageCounter(14);
            MessageCounter mc100 = new MessageCounter(100);
            MessageCounter almostMax = new MessageCounter(short.MaxValue - 100);
            MessageCounter halfMax = new MessageCounter(short.MaxValue/2 + 15);
            MessageCounter mc15 = new MessageCounter(15);
            Assert.Greater(mc100, mc14, "100 < 14");
            Assert.Greater(halfMax, mc100, "halfMax < 100");
            Assert.Greater(mc14, halfMax, "14 < halfMax");
            Assert.Greater(almostMax, halfMax, "almostMax < halfMax");
            Assert.Greater(mc100, almostMax, "100 < almostMax");
            Assert.Greater(mc14, almostMax, "14 < almostMax");


            Assert.AreEqual(mc14++, mc15, "++ not working");
        }

    }
}
