using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins.Lobby
{
    public class LobbyRoom<TAccountModel, TGame> : Room<TAccountModel, TGame>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        internal bool _gameStarted;
        internal readonly int MaxPlayerCount;
        internal readonly int MinPlayerCount;
        internal LobbyRoom(int maxPlayerCount, int minPlayerCount) : base()
        {
            Players = new List<TAccountModel>();
            MaxPlayerCount = maxPlayerCount;
            MinPlayerCount = minPlayerCount;
            _gameStarted = false;
        }
        internal void Join(TAccountModel player)
        {
            if (Players.Count < MaxPlayerCount && !_gameStarted)
            {
                Players.Add(player);
                PlayerJoined.Invoke(this, player);
            }
        }
        internal void Leave(TAccountModel player)
        {
            if (Players.Contains(player) && !_gameStarted)
            {
                Players.Remove(player);
                PlayerLeaved.Invoke(this, player);
            }
        }
        internal void StartGame()
        {
            if (Players.Count >= MinPlayerCount)
            {
                _gameStarted = true;
                InitRoom();
                GameStarted.Invoke(this);
            }
        }
        internal event Action<LobbyRoom<TAccountModel, TGame>, TAccountModel> PlayerJoined;
        internal event Action<LobbyRoom<TAccountModel, TGame>, TAccountModel> PlayerLeaved;
        internal event Action<LobbyRoom<TAccountModel, TGame>> GameStarted;
    }
}
