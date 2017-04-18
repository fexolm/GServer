using System;
using System.Collections.Generic;

namespace GServer.Plugins
{
    public abstract class Game<TAccountModel>
        where TAccountModel : AccountModel, new()
    {
        public AccountModel[] Players { get; set; }
    }

    class RoomManager<TGame, TAccountModel> : IPlugin
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        private IDictionary<Token, Room<TAccountModel, TGame>> _rooms;
        private Matchmaking<TAccountModel> _matchmaking;
        private Host _host;
        public RoomManager(Matchmaking<TAccountModel> matchmaking)
        {
            _rooms = new Dictionary<Token, Room<TAccountModel, TGame>>();
            _matchmaking = matchmaking;
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
                _host.Send(new Message((short)MatchmakingMessages.GameFound, Mode.Reliable, room.RoomToken), player.Connection);
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
            _host.AddHandler(messageType, (m, c) =>
            {
                lock (_rooms)
                {
                    if (_rooms.ContainsKey(c.Token))
                    {
                        roomHandler.Invoke(m, _rooms[c.Token]);
                    }
                }
            });
        }
    }
}
