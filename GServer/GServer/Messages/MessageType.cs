using System.IO;

namespace GServer.Messages
{
    public class MType
    {
        private byte _header { get { return new FlagContainer(Private, Reliable, Sequenced, Ordered, RequireToken).GetByte(); } }
        public bool Private { get; set; }
        public bool Reliable { get; set; }
        public bool Sequenced { get; set; }
        public bool Ordered { get; set; }
        public bool RequireToken { get; set; }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(_header);
                }
                return m.ToArray();
            }
        }
        public static MType Deserialize(byte[] buffer)
        {
            MType result = new MType();
            using (MemoryStream m = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    var fc = new FlagContainer(reader.ReadByte());
                    result.RequireToken = fc.Pop();
                    result.Ordered = fc.Pop();
                    result.Sequenced = fc.Pop();
                    result.Reliable = fc.Pop();
                    result.Private = fc.Pop();
                }
            }
            return result;
        }
    }
}
