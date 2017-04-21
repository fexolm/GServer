using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins
{
    class Lobby<TAccountModel>
        where TAccountModel : AccountModel
    {
        private TAccountModel[] _players;
        public Lobby(TAccountModel[] players, Host host)
        {
            _players = players;
        }
    }
}
