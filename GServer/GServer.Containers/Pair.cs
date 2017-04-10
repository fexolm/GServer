namespace GServer.Containers
{
    public class Pair<T1, T2>
    {
        public T1 Val1 { get; set; }
        public T2 Val2 { get; set; }

        public Pair(T1 v1, T2 v2)
        {
            Val1 = v1;
            Val2 = v2;
        }
    }
}
