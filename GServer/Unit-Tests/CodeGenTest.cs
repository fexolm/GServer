using GServer;
using GServer.Containers;
using NUnit.Framework;

namespace Unit_Tests
{
    class Sample
    {
        [DsSerialize]
        public int A { get; set; }

        [DsSerialize]
        public string B { get; set; }

        public Sample(int a, string b) {
            A = a;
            B = b;
        }

        public Sample() {
            
        }
    }

    public class CodeGenTest
    {
        [Test]
        public void SerializerTest() {
            var sample = new Sample(512, "kek");
            var serializer = CodeGen.GenerateSerializer(typeof(Sample));
            var deserializer = CodeGen.GenerateDeserializer(typeof(Sample));
            var buffer = serializer.Invoke(sample);

            var res = (Sample) deserializer.Invoke(buffer);

            Assert.AreEqual(res.A, sample.A);
            Assert.AreEqual(res.B, sample.B);
        }
    }
}