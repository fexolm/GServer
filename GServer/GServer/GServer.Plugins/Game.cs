using System;
using GServer.Messages;

namespace GServer.Plugins
{
    public abstract class Game<TAccountModel>
        where TAccountModel : AccountModel, new()
    {
        public AccountModel[] Players { get; set; }
        public Action<Message, Connection.Connection> Send;
        public abstract void InitGame();
    }
}