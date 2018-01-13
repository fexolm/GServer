namespace GServer.Containers
{
    [System.Obsolete("Use DsSerializer instead")]
    public interface IDeepSerializable
    {
        void PushToDs(DataStorage ds);
    }

    [System.Obsolete("Use DsSerializer instead")]
    public interface IDeepDeserializable
    {
        void ReadFromDs(DataStorage ds);
    }
}