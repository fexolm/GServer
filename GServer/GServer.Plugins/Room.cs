using System;

namespace GServer.Plugins
{
    public class Room<TAccountModel, TGame>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        public TGame Game;
        public readonly Token RoomToken;
        public TAccountModel[] Players;
        public Room(TAccountModel[] players)
        {
            RoomToken = Token.GenerateToken();
            Players = players;
        }
        public void InitRoom()
        {
            Game = new TGame();
            Game.Players = Players;
            Game.Send = (msg, con) =>
            {
                Send?.Invoke(msg, con);
            };
            Game.InitGame();
        }
        public Action RoomClosed;
        public void Close()
        {
            RoomClosed?.Invoke();
        }
        public Action<Message, Connection> Send;
    }
}
