using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.State
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
    public class Quanternion : Vector3 
    {
        public float W { get; set; }
    }
    public class GameObject
    {
        public bool Interacting { get; set; }

    }
}
