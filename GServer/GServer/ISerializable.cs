using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public interface ISerializable
    {
        public byte[] Serialize();
    }
}
