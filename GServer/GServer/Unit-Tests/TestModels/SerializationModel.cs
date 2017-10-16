using GServer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit_Tests.TestModels
{
    class SerializationModel : ISerializable, IDeserializable, IDeepSerializable, IDeepDeserializable
    {
        public int IntProp { get; set; }

        public double DoubleProp { get; set; }

        public string StringProp { get; set; }

        public SerializabeProp SPProp { get; set; }

        public void FillDeserialize(byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            ReadFromDs(ds);
        }

        public byte[] Serialize()
        {
            var ds = DataStorage.CreateForWrite();
            PushToDs(ds);
            return ds.Serialize();
        }

        public void PushToDs(DataStorage ds)
        {
            ds.Push(IntProp).Push(DoubleProp).Push(StringProp).Push(SPProp);
        }

        public void ReadFromDs(DataStorage ds)
        {
            SPProp = new SerializabeProp();
            IntProp = ds.ReadInt32();
            DoubleProp = ds.ReadDouble();
            StringProp = ds.ReadString();
            SPProp.ReadFromDs(ds);
        }

        public class SerializabeProp : IDeepSerializable, IDeepDeserializable
        {
            public List<int> ListIntProp { get; set; }

            public string StringProp { get; set; }

            public List<SerializableProp2> ListSP2Prop { get; set; }

            public void PushToDs(DataStorage ds)
            {
                ListIntProp.SerializeTo(ds);
                ds.Push(StringProp);
                ListSP2Prop.SerializeTo(ds);
            }

            public void ReadFromDs(DataStorage ds)
            {
                ListIntProp = new List<int>();
                ListSP2Prop = new List<SerializableProp2>();
                ListIntProp.DeserializeFrom(ds);
                StringProp = ds.ReadString();
                ListSP2Prop.DeserializeFrom(ds);
            }

            public class SerializableProp2 : IDeepSerializable, IDeepDeserializable
            {
                public bool BoolProp { get; set; }

                public List<string> ListStringProp { get; set; }

                public void PushToDs(DataStorage ds)
                {
                    ds.Push(BoolProp);
                    ListStringProp.SerializeTo(ds);
                }

                public void ReadFromDs(DataStorage ds)
                {
                    BoolProp = ds.ReadBoolean();
                    ListStringProp = new List<string>();
                    ListStringProp.DeserializeFrom(ds);
                }

                public override bool Equals(object obj)
                {
                    var other = (SerializableProp2)obj;
                    bool answer = true;
                    return base.Equals(obj);
                }
            }
        }
    }
}
