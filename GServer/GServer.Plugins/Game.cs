using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins
{
    public abstract class Game<TAccountModel>
        where TAccountModel : AccountModel, new()
    {
        public AccountModel[] Players { get; set; }
        public Action<Message, Connection> Send;
        public abstract void InitGame();
    }
}
