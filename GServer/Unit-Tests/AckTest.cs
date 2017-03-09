using GServer;
using NUnit.Framework;
using System.Collections.Generic;

namespace Unit_Tests
{
    public class AckTest
    {
        [Test]
        public void IfNoLostPackages()
        {
            Ack ack = new Ack();
            bool packetLost = false;
            ack.PacketLost += (p) => packetLost = true;
            for (MessageCounter mc = 0; mc < 32; mc++)
            {
                Assert.AreEqual(-1, ack.GetStatistic(mc));
            }
            Assert.AreEqual(false, packetLost);
        }

        [Test]
        public void IfSomePackageNotEvenSent()
        {
            Ack ack = new Ack();
            int lostPackagesCount = 0;
            ack.PacketLost += (p) => lostPackagesCount++;
            for (MessageCounter mc = 0; mc < 16; mc++)
            {
                Assert.AreEqual(-1, ack.GetStatistic(mc));
            }
            int i = 0;
            for (MessageCounter mc = 17; mc < 32; mc++)
            {
                Assert.AreEqual((~(1 << ++i)), ack.GetStatistic(mc));
            }
            Assert.AreEqual(0, lostPackagesCount);
        }
        [Test]
        public void IfSomePackageLost()
        {
            Ack ack = new Ack();
            int lostPackagesCount = 0;
            ack.PacketLost += (p) => lostPackagesCount++;
            for (MessageCounter mc = 0; mc < 16; mc++)
            {
                Assert.AreEqual(-1, ack.GetStatistic(mc));
            }
            int i = 0;
            for (MessageCounter mc = 17; mc < 17 + 31; mc++)
            {
                Assert.AreEqual((~(1 << ++i)), ack.GetStatistic(mc));
            }
            MessageCounter lostCounter = 17 + 31;
            Assert.AreEqual(-1, ack.GetStatistic(lostCounter));
            Assert.AreEqual(1, lostPackagesCount);
        }
        [Test]
        public void IfSomePackageAlmostLost()
        {
            Ack ack = new Ack();
            int lostPackagesCount = 0;
            ack.PacketLost += (p) => lostPackagesCount++;
            for (MessageCounter mc = 0; mc < 16; mc++)
            {
                Assert.AreEqual(-1, ack.GetStatistic(mc));
            }
            int i = 0;
            for (MessageCounter mc = 17; mc < 17 + 29; mc++)
            {
                Assert.AreEqual((~(1 << ++i)), ack.GetStatistic(mc));
            }
            Assert.AreEqual(-1, ack.GetStatistic(16));
            for (MessageCounter mc = 17 + 29; mc < 100; mc++)
            {
                Assert.AreEqual(-1, ack.GetStatistic(mc));
            }
            Assert.AreEqual(0, lostPackagesCount);
        }
        [Test]
        public void IfManyPackagesLost()
        {
            Ack ack = new Ack();
            List<MessageCounter> lostPackages = null;
            ack.PacketLost += (p) => lostPackages = p;
            Assert.AreEqual(-1, ack.GetStatistic(1));
            Assert.AreEqual(1, ack.GetStatistic(65));
            Assert.AreEqual(32, lostPackages.Count);
        }
    }
}
