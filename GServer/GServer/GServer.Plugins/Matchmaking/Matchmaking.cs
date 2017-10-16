using System;
using GServer.Connection;
using GServer.Containers;
using GServer.Messages;

namespace GServer.Plugins.Matchmaking
{
    [Reserve(3000, 3020)]
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

    public class Matchmaking<TAccountModel> : GameSearcher<TAccountModel>
        where TAccountModel : AccountModel, new()
    {
        private PlayerQueue<TAccountModel> _playerQueue;

        public Matchmaking(Account<TAccountModel> account, PlayerQueue<TAccountModel> playerQueue) : base(account) {
            _playerQueue = playerQueue;
            _playerQueue.OnGameFound += ValidateGameSession;
        }

        public Matchmaking(Account<TAccountModel> account) : base(account) { }

        private void ValidateGameSession(TAccountModel[] players) {
            var lobby = new MatchWaiting<TAccountModel>(players, _host);
            lobby.OnValidateSuccess += CreateGameSession;
            lobby.BackToQueue += ReturnToQueue;
        }

        private void ReturnToQueue(MatchWaiting<TAccountModel> sender, TAccountModel[] players) {
            sender.BackToQueue -= ReturnToQueue;
            sender.OnValidateSuccess -= CreateGameSession;
            foreach (var p in players) {
                _playerQueue.AddPlayer(p);
            }
        }

        private void CreateGameSession(MatchWaiting<TAccountModel> sender, TAccountModel[] players) {
            sender.BackToQueue -= ReturnToQueue;
            sender.OnValidateSuccess -= CreateGameSession;
            if (RoomCreated != null)
                RoomCreated.Invoke(players);
        }

        public void FindGame() {
            _host.Send(new Message((short) MatchmakingMessages.MatchmakingRequest, Mode.Reliable));
        }

        private Token _roomToken;

        protected override void InitializeHandlers() {
            _host.AddHandler((short) MatchmakingMessages.GameFound, (m, c) => {
                var ds = DataStorage.CreateForRead(m.Body);
                _roomToken = new Token();
                _roomToken.ReadFromDs(ds);
                OnGameFound.Invoke();
            });
            _host.AddHandler((short) MatchmakingMessages.ValidateActivity,
                (m, c) => {
                    _host.Send(new Message((short) MatchmakingMessages.ValidateActivitySuccess, Mode.Reliable));
                });
            _account.AddHandler((short) MatchmakingMessages.MatchmakingRequest, (m, a) => {
                Console.WriteLine("matchmaking request from {0}", a.Connection.EndPoint.ToString());
                lock (_playerQueue) {
                    _playerQueue.AddPlayer(a);
                }
            });
            _account.AddHandler((short) MatchmakingMessages.CancelMatchmaking, (m, a) => {
                lock (_playerQueue) {
                    _playerQueue.RemovePlayer(a);
                }
            });
        }

        public void Send(Message msg) {
            var ds = DataStorage.CreateForWrite();
            ds.Push(_roomToken);
            ds.Push(msg.Body);
            msg.Body = ds.Serialize();
            _host.Send(msg);
        }

        public event Action OnGameFound;
        public Action<TAccountModel[]> RoomCreated;
    }
}