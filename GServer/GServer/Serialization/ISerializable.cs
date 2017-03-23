
namespace GServer
{
    public interface ISerializable
    {
        byte[] Serialize();
    }
    public interface IDeserializable
    {
        void FillDeserialize(byte[] buffer);
    }
}
