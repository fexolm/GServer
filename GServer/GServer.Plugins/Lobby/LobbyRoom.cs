using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins.Lobby
{
    class LobbyRoom<TAccountModel, TGame> : Room<TAccountModel, TGame>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        private bool _gameStarted;
        public readonly int MaxPlayerCount;
        public readonly int MinPlayerCount;
        private List<TAccountModel> _players = new List<TAccountModel>();
        public override List<TAccountModel> Players
        {
            get
            {
                return _players;
            }
            set
            {
                _players = value;
            }
        }
        public LobbyRoom(int maxPlayerCount, int minPlayerCount) : base()
        {
            MaxPlayerCount = maxPlayerCount;
            MinPlayerCount = minPlayerCount;
            _gameStarted = false;
        }
        public void Join(TAccountModel player)
        {
            if (_players.Count < MaxPlayerCount && !_gameStarted)
            {
                _players.Add(player);
                PlayerJoined.Invoke(this, player);
            }
        }
        public void Leave(TAccountModel player)
        {
            if (_players.Contains(player) && !_gameStarted)
            {
                _players.Remove(player);
                PlayerLeaved.Invoke(this, player);
            }
        }
        public void StartGame()
        {
            if (_players.Count >= MinPlayerCount)
            {
                _gameStarted = true;
                GameStarted.Invoke(this);
            }
        }
        public event Action<LobbyRoom<TAccountModel, TGame>, TAccountModel> PlayerJoined;
        public event Action<LobbyRoom<TAccountModel, TGame>, TAccountModel> PlayerLeaved;
        public event Action<LobbyRoom<TAccountModel, TGame>> GameStarted;
    }
}
