using System;

namespace GServer.Plugins
{
    public class Room<TAccountModel, TGame>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        public readonly TGame Game;
        public readonly Token RoomToken;
        public TAccountModel[] Players;
        public Room(TAccountModel[] players)
        {
            RoomToken = Token.GenerateToken();
            Players = players;
            Game = new TGame();
            Game.Players = players;
        }
        public Action RoomClosed;
        public void Close()
        {
            RoomClosed?.Invoke();
        }
    }
}
