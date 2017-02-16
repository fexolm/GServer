using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Messages
{
    class MessageManager
    {
        private readonly Host _host;
        public MessageManager(Host host)
        {
            _host = host;
        }
    }
}
