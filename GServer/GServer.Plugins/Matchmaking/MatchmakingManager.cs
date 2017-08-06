using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins.Matchmaking
{
    public class MatchmakingManager<TGame, TAccountModel> : RoomManager<TGame, TAccountModel, MatchmakingRoom<TAccountModel, TGame>>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        private Matchmaking<TAccountModel> _matchmaking;
        public MatchmakingManager(Matchmaking<TAccountModel> matchmaking)
        {
            _matchmaking = matchmaking;
            _matchmaking.RoomCreated += ManageRoom;
        }
        private void ManageRoom(TAccountModel[] clients)
        {
            var room = new MatchmakingRoom<TAccountModel, TGame>(clients);
            room.Send = (msg, con) => _host.Send(msg, con);
            room.InitRoom();
            lock (_rooms)
            {
                _rooms.Add(room.RoomToken, room);
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
    }
}
