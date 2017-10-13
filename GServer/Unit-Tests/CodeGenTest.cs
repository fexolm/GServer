﻿using GServer;
using GServer.Containers;
using NUnit.Framework;

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

        public Sample(int a, string b, SampleIn c) {
            A = a;
            B = b;
            C = c;
        }

        public Sample() { }
    }

    public class CodeGenTest
    {
        [Test]
        public void SerializerTest() {
            var sample = new Sample(512, "kek", new SampleIn(123, "lol"));
            var buffer = FuckingAwesomeDataStorageSerializer.Serialize(sample);

            var res = FuckingAwesomeDataStorageSerializer.DeserializeInto<Sample>(buffer);

            Assert.AreEqual(res.A, sample.A);
            Assert.AreEqual(res.B, sample.B);
        }
    }
}