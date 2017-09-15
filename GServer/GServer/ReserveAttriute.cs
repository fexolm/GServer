using System;

namespace GServer
{
    public class ReserveAttribute : Attribute
    {
        public readonly int Start;
        public readonly int End;

        public ReserveAttribute(int start, int end) {
            Start = start;
            End = end;
        }
    }
}