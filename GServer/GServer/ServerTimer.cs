using System;

namespace GServer
{
    static class ServerTimer
    {
        public static void Tick()
        {
            OnTick?.Invoke();
        }
        public static event Action OnTick;
    }
}
