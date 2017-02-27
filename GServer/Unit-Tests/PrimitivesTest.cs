using GServer;
using GServer.Messages;
using NUnit.Framework;

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
        [Test]
        public void FlagContainerTest()
        {
            FlagContainer fc = new FlagContainer(true, true, false, false, true);
            Assert.AreEqual(5, fc.Length);
            Assert.AreEqual(true, fc.Pop());
            Assert.AreEqual(false, fc.Pop());
            Assert.AreEqual(false, fc.Pop());
            Assert.AreEqual(true, fc.Pop());
            Assert.AreEqual(true, fc.Pop());
            Assert.AreEqual(0, fc.Length);
        }
        [Test]
        public void MessageTypeTest()
        {
            MType type = new MType();
            type.Reliable = true;
            type.Sequenced = false;
            type.Ordered = false;
            type.Private = true;
            type.RequireToken = true;
            var bytes = type.Serialize();
            var type2 = MType.Deserialize(bytes);
            Assert.AreEqual(type.Reliable, type2.Reliable);
            Assert.AreEqual(type.Ordered, type2.Ordered);
            Assert.AreEqual(type.Sequenced, type2.Sequenced);
            Assert.AreEqual(type.Private, type2.Private);
            Assert.AreEqual(type.RequireToken, type2.RequireToken);
        }
    }
}
