using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins.Lobby
{
    class LobbyManager<TAccountModel, TGame> : RoomManager<TGame, TAccountModel, LobbyRoom<TAccountModel, TGame>>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        public void CreateRoom(TAccountModel host, int maxPlayers)
        {
            var room = new LobbyRoom<TAccountModel, TGame>(maxPlayers);
            room.Join(host);
            room.PlayerJoined += PlayerJoinedHandler;
            room.PlayerLeaved += PlayerLeavedHander;
            lock (_rooms)
            {
                if (!_rooms.ContainsKey(host.Connection.Token))
                {
                }
            }
        }

        private void PlayerLeavedHander(LobbyRoom<TAccountModel, TGame> room, TAccountModel player)
        {
            lock (_rooms)
            {

            }
        }

        private void PlayerJoinedHandler(LobbyRoom<TAccountModel, TGame> room, TAccountModel player)
        {
            throw new NotImplementedException();
        }
    }
}
