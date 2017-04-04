using System;
using System.Collections.Generic;

namespace GServer.Plugins
{
    public abstract class Game<TAccountModel>
        where TAccountModel : AccountModel, new()
    {
        public AccountModel[] Players { get; set; }


    }

    public class RoomManager<TGame, TAccountModel> : IPlugin
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        private IDictionary<Token, Room<TAccountModel, TGame>> _rooms;
        private Matchmaking<TAccountModel> _matchmaking;
        private Host _host;
        private Account<TAccountModel> _account;
        public RoomManager(Matchmaking<TAccountModel> matchmaking, Account<TAccountModel> account)
        {
            _matchmaking = matchmaking;
            _account = account;
            _matchmaking.RoomCreated += ManageRoom;
        }
        private void ManageRoom(TAccountModel[] clients)
        {
            var room = new Room<TAccountModel, TGame>(clients);
            lock (_rooms)
            {
                foreach (var client in clients)
                {
                    _rooms.Add(client.Connection.Token, room);
                }
            }
            foreach (var player in room.Players)
            {
                _host.Send(new Message((short)MatchmakingMessages.GameFound, Mode.Reliable), player.Connection);
            }
            room.RoomClosed += () =>
            {
                lock (_rooms)
                {
                    foreach (var player in room.Players)
                    {
                        if (_rooms.ContainsKey(player.Connection.Token))
                        {
                            _rooms.Remove(player.Connection.Token);
                        }
                    }
                }
            };
        }
        public void Bind(Host host)
        {
            _host = host;
        }
        public void AddHandler(short messageType, Action<Message, Room<TAccountModel, TGame>> roomHandler)
        {
            _account.AddHandler(messageType, (m, a) =>
             {
                 lock (_rooms)
                 {
                     if (_rooms.ContainsKey(a.Connection.Token))
                     {
                         roomHandler.Invoke(m, _rooms[a.Connection.Token]);
                     }
                 }
             });
        }
    }
}
