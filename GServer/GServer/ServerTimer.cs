using System;

namespace GServer
{
    public static class ServerTimer
    {
        internal static void Tick()
        {
            OnTick?.Invoke();
        }
        public static event Action OnTick;
    }
}
