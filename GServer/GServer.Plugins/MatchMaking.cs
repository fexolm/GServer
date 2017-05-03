using System;
using GServer.Containers;
namespace GServer.Plugins
{
    public enum MatchmakingMessages
    {
        GameFound = 3000,
        MatchmakingRequest = 3001,
        CancelMatchmaking = 3002,
        RoomClosed = 3003,
        ValidateActivity = 3004,
        ValidateActivitySuccess = 3005,
    }
    public abstract class PlayerQueue<TAccountModel>
        where TAccountModel : AccountModel, new()
    {
        public abstract void AddPlayer(TAccountModel player);
        public abstract void RemovePlayer(TAccountModel player);
        public Action<TAccountModel[]> OnGameFound;
    }
    public class Matchmaking<TAccountModel> : IPlugin
        where TAccountModel : AccountModel, new()
    {
        private Host _host;
        private Account<TAccountModel> _account;
        private PlayerQueue<TAccountModel> _playerQueue;
        public Matchmaking(Account<TAccountModel> account, PlayerQueue<TAccountModel> playerQueue)
        {
            _account = account;
            _playerQueue = playerQueue;
            _playerQueue.OnGameFound += ValidateGameSession;
        }
        public Matchmaking(Account<TAccountModel> account)
        {
            _account = account;
        }
        public void Bind(Host host)
        {
            _host = host;
            _host.AddHandler((short)MatchmakingMessages.GameFound, (m, c) =>
            {
                var ds = DataStorage.CreateForRead(m.Body);
                OnGameFound.Invoke(new Token(ds.ReadInt32()));
            });
            _host.AddHandler((short)MatchmakingMessages.ValidateActivity, (m, c) =>
            {
                _host.Send(new Message((short)MatchmakingMessages.ValidateActivitySuccess, Mode.Reliable));
            });
            _account.AddHandler((short)MatchmakingMessages.MatchmakingRequest, (m, a) =>
            {
                Console.WriteLine("matchmaking request from {0}", a.Connection.EndPoint.ToString());
                lock (_playerQueue)
                 {
                     _playerQueue.AddPlayer(a);
                 }
            });
            _account.AddHandler((short)MatchmakingMessages.CancelMatchmaking, (m, a) =>
            {
                lock (_playerQueue)
                {
                    _playerQueue.RemovePlayer(a);
                }
            });
        }
        private void ValidateGameSession(TAccountModel[] players)
        {
            Lobby<TAccountModel> lobby = new Lobby<TAccountModel>(players, _host);
            lobby.OnValidateSuccess += CreateGameSession;
            lobby.BackToQueue += ReturnToQueue;
        }

        private void ReturnToQueue(Lobby<TAccountModel> sender, TAccountModel[] players)
        {
            sender.BackToQueue -= ReturnToQueue;
            sender.OnValidateSuccess -= CreateGameSession;
            foreach (var p in players)
            {
                _playerQueue.AddPlayer(p);
            }
        }

        private void CreateGameSession(Lobby<TAccountModel> sender, TAccountModel[] players)
        {
            sender.BackToQueue -= ReturnToQueue;
            sender.OnValidateSuccess -= CreateGameSession;
           if (RoomCreated!=null) RoomCreated.Invoke(players);
        }

        public void FindGame()
        {
            _host.Send(new Message((short)MatchmakingMessages.MatchmakingRequest, Mode.Reliable));
        }
        public event Action<Token> OnGameFound;
        public event Action<TAccountModel[]> RoomCreated;
    }
}
