using GServer.Containers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unit_Tests.TestModels;

namespace Unit_Tests
{
    class SerializationTest
    {
        [Test]
        public void DataStorageTest() {
            DataStorage ds = DataStorage.CreateForWrite();
            ds.Push(13);
            ds.Push("word");
            ds.Push(true);
            ds.Push("hello world");
            ds.Push(13.221F);
            ds.Push(14.32D);

            DataStorage readDs = DataStorage.CreateForRead(ds.Serialize());

            Assert.AreEqual(13, readDs.ReadInt32());
            Assert.AreEqual("word", readDs.ReadString());
            Assert.AreEqual(true, readDs.ReadBoolean());
            Assert.AreEqual("hello world", readDs.ReadString());
            Assert.AreEqual(13.221F, readDs.ReadFloat());
            Assert.AreEqual(14.32D, readDs.ReadDouble());
        }

        [Test]
        public void ListSerializationTest_Int() {
            var list = new List<int> {5, 4, 2, 3, 100};
            var a = list.Serialize();
            var dList = new List<int>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<int>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void ListSerializationTest_Short() {
            var list = new List<short> {5, 4, 2, 3, 100};
            var a = list.Serialize();
            var dList = new List<short>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<short>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void ListSerializationTest_Long() {
            var list = new List<long> {533, 432132, 22314, 3, 100};
            var a = list.Serialize();
            var dList = new List<long>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<long>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void ListSerializationTest_Double() {
            var list = new List<double> {5.23, 4.41, 2.77, 3.1, 100};
            var a = list.Serialize();
            var dList = new List<double>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<double>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void ListSerializationTest_Float() {
            var list = new List<float> {5.3121f, 4.213f, 2.1f, 3f, 100.0f};
            var a = list.Serialize();
            var dList = new List<float>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<float>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void ListSerializationTest_String() {
            var list = new List<string> {"string", "check", "test"};
            var a = list.Serialize();
            var dList = new List<string>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<string>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void ListSerializationTest_Char() {
            var list = new List<char> {'s', 't', 'r', 'i', 'n', 'g'};
            var a = list.Serialize();
            var dList = new List<char>();
            dList.FillDeserialize(a);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);

            dList = new List<char>();
            var dsW = DataStorage.CreateForWrite();
            list.SerializeTo(dsW);
            var dsR = DataStorage.CreateForRead(dsW.Serialize());
            dList.DeserializeFrom(dsR);
            Assert.AreEqual(list.Count, dList.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], dList[i]);
        }

        [Test]
        public void DeepSerializationTest() {
            SerializationModel sm = new SerializationModel {
                IntProp = 5,
                DoubleProp = 5.23,
                StringProp = "string",
                SPProp = new SerializationModel.SerializabeProp {
                    ListIntProp = new List<int> {1, 2, 3, 4, 5},
                    StringProp = "test",
                    ListSP2Prop = new List<SerializationModel.SerializabeProp.SerializableProp2> {
                        new SerializationModel.SerializabeProp.SerializableProp2 {
                            BoolProp = true,
                            ListStringProp = new List<string> {"check", "list", "data"}
                        },
                        new SerializationModel.SerializabeProp.SerializableProp2 {
                            BoolProp = false,
                            ListStringProp = new List<string> {"mana", "healthpoints", "exp"}
                        },
                        new SerializationModel.SerializabeProp.SerializableProp2 {
                            BoolProp = true,
                            ListStringProp = new List<string> {"damage", "heal", "monk"}
                        }
                    }
                }
            };

            var bytes = sm.Serialize();
            var dsm = new SerializationModel();
            dsm.FillDeserialize(bytes);
            Assert.AreEqual(sm.IntProp, dsm.IntProp);
            Assert.AreEqual(sm.DoubleProp, dsm.DoubleProp);
            Assert.AreEqual(sm.StringProp, dsm.StringProp);
            Assert.AreEqual(sm.SPProp.StringProp, dsm.SPProp.StringProp);
            Assert.AreEqual(sm.SPProp.ListIntProp.Count, dsm.SPProp.ListIntProp.Count);
            for (int i = 0; i < sm.SPProp.ListIntProp.Count; i++)
                Assert.AreEqual(sm.SPProp.ListIntProp[i], dsm.SPProp.ListIntProp[i]);
            Assert.AreEqual(sm.SPProp.ListSP2Prop.Count, sm.SPProp.ListSP2Prop.Count);
            for (int i = 0; i < sm.SPProp.ListSP2Prop.Count; i++)
                sm.SPProp.ListSP2Prop[i].Equals(dsm.SPProp.ListSP2Prop[i]);
        }

        [Test]
        public void DictionaryTest() {
            var dct = new Dictionary<int, int> {{15, 42}, {5, 90}, {19, 6}, {100, 48}};
            var ds = DataStorage.CreateForWrite();
            dct.SerializeTo(ds);
            var nds = DataStorage.CreateForRead(ds.Serialize());
            var ndct = new Dictionary<int, int>();
            ndct.DeserializeFrom(nds);
            Assert.AreEqual(dct.Count, ndct.Count);
            var keys = dct.Keys.ToList();
            var nkeys = ndct.Keys.ToList();
            var vals = dct.Values.ToList();
            var nvals = ndct.Values.ToList();

            for (int i = 0; i < dct.Count; i++) {
                Assert.AreEqual(keys[i], nkeys[i]);
                Assert.AreEqual(vals[i], nvals[i]);
            }
        }
    }
}