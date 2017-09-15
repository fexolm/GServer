namespace GServer.Containers
{
    public class Vector3 : ISerializable, IDeserializable, IDeepSerializable, IDeepDeserializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3() {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector3(float x, float y, float z) {
            X = x;
            Y = y;
            Z = z;
        }

        public void FillDeserialize(byte[] buffer) {
            var ds = DataStorage.CreateForRead(buffer);
            ReadFromDs(ds);
        }

        public void PushToDs(DataStorage ds) {
            ds.Push(X).Push(Y).Push(Z);
        }

        public void ReadFromDs(DataStorage ds) {
            X = ds.ReadFloat();
            Y = ds.ReadFloat();
            Z = ds.ReadFloat();
        }

        public byte[] Serialize() {
            var ds = DataStorage.CreateForWrite();
            PushToDs(ds);
            return ds.Serialize();
        }
    }
}