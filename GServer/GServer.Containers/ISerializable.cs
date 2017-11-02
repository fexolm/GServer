namespace GServer.Containers
{
    [System.Obsolete("Use DsSerializer instead")]
    public interface ISerializable
    {
        byte[] Serialize();
    }

    [System.Obsolete("Use DsSerializer instead")]
    public interface IDeserializable
    {
        void FillDeserialize(byte[] buffer);
    }
}