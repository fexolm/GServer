using System;
using System.Collections.Generic;
using System.Linq;

namespace GServer.Plugins
{
    public class Room<TAccountModel, TGame>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        public TGame Game;
        public readonly Token RoomToken;
        public List<TAccountModel> Players { get; set; }
        internal Room()
        {
            RoomToken = Token.GenerateToken();
        }
        internal void InitRoom()
        {
            Game = new TGame();
            Game.Players = Players.ToArray();
            Game.Send = (msg, con) =>
            {
                if (Send != null) Send.Invoke(msg, con);
            };
            Game.InitGame();
        }
        public Action RoomClosed;
        internal void Close()
        {
            if (RoomClosed != null) RoomClosed.Invoke();
        }
        public Action<Message, Connection> Send;
    }
}
