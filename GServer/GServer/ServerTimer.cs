using System;

namespace GServer
{
    public static class ServerTimer
    {
        internal static void Tick()
        {
           if (OnTick!=null) OnTick.Invoke();
        }
        public static event Action OnTick;
    }
}
