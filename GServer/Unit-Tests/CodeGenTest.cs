using System.Runtime.InteropServices;
using GServer;
using GServer.Containers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Unit_Tests
{
    class SampleIn
    {
        [DsSerialize]
        public int A1 { get; set; }

        [DsSerialize]
        public string B1 { get; set; }

        public SampleIn(int a, string b) {
            A1 = a;
            B1 = b;
        }

        public SampleIn() { }
    }

    class Sample
    {
        [DsSerialize]
        public int A { get; set; }

        [DsSerialize]
        public string B { get; set; }

        [DsSerialize]
        public SampleIn C { get; set; }

        [DsSerialize]
        public List<SampleIn> D { get; set; }

        public Sample(int a, string b, SampleIn c, List<SampleIn> d) {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public Sample() { }
    }

    public class CodeGenTest
    {
        [Test]
        public void SerializerTest() {
            var sample = new Sample(512, "kek", new SampleIn(123, "431"), new List<SampleIn> {
                new SampleIn(123, "kok"),
                new SampleIn(154, "lol"),
                new SampleIn(125, "heh"),
            });
            var buffer = DsSerializer.Serialize(sample);

            var res = DsSerializer.DeserializeInto<Sample>(buffer);

            Assert.AreEqual(res.A, sample.A);
            Assert.AreEqual(res.B, sample.B);
            Assert.AreEqual(res.C.A1, sample.C.A1);
            Assert.AreEqual(res.C.B1, sample.C.B1);
            foreach (var val in res.D.Zip(sample.D, (f, s) => new {first = f, second = s})) {
                Assert.AreEqual(val.first.A1, val.second.A1);
                Assert.AreEqual(val.first.B1, val.second.B1);
            }
        }
    }
}