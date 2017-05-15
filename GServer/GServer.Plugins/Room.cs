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
        public virtual List<TAccountModel> Players { get; set; }
        public Room()
        {
            RoomToken = Token.GenerateToken();
        }
        public void InitRoom()
        {
            Game = new TGame();
            Game.Players = Players.ToArray();
            Game.Send = (msg, con) =>
            {
               if (Send!=null) Send.Invoke(msg, con);
            };
            Game.InitGame();
        }
        public Action RoomClosed;
        public void Close()
        {
           if (RoomClosed!=null) RoomClosed.Invoke();
        }
        public Action<Message, Connection> Send;
    }
}
