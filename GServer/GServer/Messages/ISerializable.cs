using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Messages
{
    public interface ISerializable
    {
        byte[] Serialize();
    }
}
