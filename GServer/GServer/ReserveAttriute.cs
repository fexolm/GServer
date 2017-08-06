using GServer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public class ReserveAttribute : Attribute
    {
        public readonly int Start;
        public readonly int End;

        public ReserveAttribute(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
