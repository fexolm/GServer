using System;
using System.Collections.Generic;

namespace GServer.Plugins
{
    public class Player
    {
        public readonly Connection Connection;
        public Player(Connection connection)
        {
            Connection = connection;
            connection.Disconnected += () => Disconnected.Invoke();
        }
        public event Action Disconnected;
    }

    public abstract class PlayerQueue
    {
        public abstract void AddPlayer(Player player);
        public abstract void RemovePlayer(Connection connection);
        public Action<Player[]> OnGameFound;
    }

    public class Room
    {
        public readonly Token RoomToken;
        private Player[] _players;
        public Room(Player[] players)
        {
            RoomToken = Token.GenerateToken();
            _players = players;
        }
        public event Action RoomClosed;
    }

    public class Matchmaking : IPlugin
    {
        enum MatchmakingMessages
        {
            GameFound = 3000,
            MatchmakingRequest = 3001,
            CancelMatchmaking = 3002,
            RoomClosed = 3003
        }


        private Host _host;
        private PlayerQueue _playerQueue;
        private IDictionary<Token, Room> _rooms;

        public Matchmaking(PlayerQueue playerQueue)
        {
            _playerQueue = playerQueue;
            _rooms = new Dictionary<Token, Room>();
            _playerQueue.OnGameFound += CreateGameSession;
        }

        public void Bind(Host host)
        {
            _host = host;
            _host.AddHandler((short)MatchmakingMessages.GameFound, (m, c) =>
            {
                var ds = new DataStorage(m.Body);
                OnGameFound.Invoke(new Token(ds.ReadInt32()));
            });
            _host.AddHandler((short)MatchmakingMessages.MatchmakingRequest, (m, c) =>
            {
                lock (_playerQueue)
                {
                    _playerQueue.AddPlayer(new Player(c));
                }
            });
            _host.AddHandler((short)MatchmakingMessages.CancelMatchmaking, (m, c) =>
            {
                lock (_playerQueue)
                {
                    _playerQueue.RemovePlayer(c);
                }
            });
        }

        private void CreateGameSession(Player[] players)
        {
            var room = new Room(players);
            lock (_rooms)
            {
                _rooms.Add(room.RoomToken, room);
            }
            foreach (var player in players)
            {
                _host.Send(new Message((short)MatchmakingMessages.GameFound, Mode.Reliable, room.RoomToken), player.Connection);
            }
        }

        public void FindGame()
        {
            _host.Send(new Message((short)MatchmakingMessages.MatchmakingRequest, Mode.Reliable));
        }
        public event Action<Token> OnGameFound;
    }
}
