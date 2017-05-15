using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins.Matchmaking
{
    public class MatchmakingRoom<TAccountModel, TGame> : Room<TAccountModel, TGame>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        public MatchmakingRoom(TAccountModel[] players) : base()
        {
            Players = players.ToList();
        }
    }
}
