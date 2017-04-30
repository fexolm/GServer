using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Containers
{
    public class Quaternion : ISerializable, IDeserializable, IDeepSerializable, IDeepDeserializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public void FillDeserialize(byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            ReadFromDs(ds);
        }

        public void PushToDs(DataStorage ds)
        {
            ds.Push(X).Push(Y).Push(Z).Push(W);
        }

        public void ReadFromDs(DataStorage ds)
        {
            X = ds.ReadFloat();
            Y = ds.ReadFloat();
            Z = ds.ReadFloat();
            W = ds.ReadFloat();
        }

        public byte[] Serialize()
        {
            var ds = new DataStorage();
            PushToDs(ds);
            return ds.Serialize();
        }
    }
}
