namespace GServer.Containers
{
    public class Quaternion : ISerializable, IDeserializable, IDeepSerializable, IDeepDeserializable
    {
        [DsSerialize]
        public float X { get; set; }

        [DsSerialize]
        public float Y { get; set; }

        [DsSerialize]
        public float Z { get; set; }

        [DsSerialize]
        public float W { get; set; }

        [System.Obsolete("Use DsSerializer instead")]
        public void FillDeserialize(byte[] buffer) {
            var ds = DataStorage.CreateForRead(buffer);
            ReadFromDs(ds);
        }

        [System.Obsolete("Use DsSerializer instead")]
        public void PushToDs(DataStorage ds) {
            ds.Push(X).Push(Y).Push(Z).Push(W);
        }

        [System.Obsolete("Use DsSerializer instead")]
        public void ReadFromDs(DataStorage ds) {
            X = ds.ReadFloat();
            Y = ds.ReadFloat();
            Z = ds.ReadFloat();
            W = ds.ReadFloat();
        }

        [System.Obsolete("Use DsSerializer instead")]
        public byte[] Serialize() {
            var ds = DataStorage.CreateForWrite();
            PushToDs(ds);
            return ds.Serialize();
        }
    }
}