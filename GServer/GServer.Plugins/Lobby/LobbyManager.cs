using GServer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins.Lobby
{
    internal enum LobbyMessages
    {
        CreateRoom = 3000,
        JoinRoom = 3001,
        LeaveRoom = 3002,
        RoomCreated = 3003,
        SuccesfullyJoined = 3004,
        SuccesfullyLeaved = 3005,
        PlayerJoined = 3006,
        PlayerLeaved = 3007,
        GameStarted = 3008,
        GetRooms = 3009,
        GetRoomsResponse = 3010,
        StartGame = 3011,
    }
    public class LobbyManager<TAccountModel, TGame> : RoomManager<TGame, TAccountModel, LobbyRoom<TAccountModel, TGame>>
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
    {
        private Account<TAccountModel> _accountManager;
        public LobbyManager(Account<TAccountModel> accountManager)
        {
            _accountManager = accountManager;
        }
        private void CreateRoom(TAccountModel host)
        {
            var room = new LobbyRoom<TAccountModel, TGame>(2, 2);
            room.PlayerJoined += PlayerJoinedHandler;
            room.PlayerLeaved += PlayerLeavedHander;
            room.GameStarted += GameStaredHandler;
            room.Join(host);
            lock (_rooms)
            {
                if (!_rooms.ContainsKey(host.Connection.Token))
                {
                    _rooms.Add(room.RoomToken, room);
                    _host.Send(new Message((short)LobbyMessages.RoomCreated, Mode.Reliable, DataStorage.CreateForWrite().Push(room.RoomToken.ToInt())), host.Connection);
                }
            }
        }
        private void GameStaredHandler(LobbyRoom<TAccountModel, TGame> room)
        {
            foreach (var p in room.Players)
            {
                _host.Send(new Message((short)LobbyMessages.GameStarted, Mode.Reliable, DataStorage.CreateForWrite().Push(room.RoomToken.ToInt())), p.Connection);
            }
        }
        protected override void InitializeHandlers()
        {
            _host.AddHandler((short)LobbyMessages.CreateRoom, (m, c) =>
            {
                var acc = _accountManager.GetAccount(c.Token);
                CreateRoom(acc);
            });

            _host.AddHandler((short)LobbyMessages.JoinRoom, (m, c) =>
            {
                var acc = _accountManager.GetAccount(c.Token);
                var ds = DataStorage.CreateForRead(m.Body);
                var roomToken = new Token(ds.ReadInt32());
                if (_rooms.ContainsKey(roomToken))
                {
                    _rooms[roomToken].Join(acc);
                }
            });

            _host.AddHandler((short)LobbyMessages.LeaveRoom, (m, c) =>
            {
                var acc = _accountManager.GetAccount(c.Token);
                var ds = DataStorage.CreateForRead(m.Body);
                var roomToken = new Token(ds.ReadInt32());
                if (_rooms.ContainsKey(roomToken))
                {
                    _rooms[roomToken].Leave(acc);
                }
            });
            _host.AddHandler((short)LobbyMessages.GetRooms, (m, c) =>
            {
                _host.Send(new Message((short)LobbyMessages.GetRoomsResponse, Mode.Reliable, DataStorage.CreateForWrite().Push(_rooms.Keys.Serialize())), c);

            });
            AddHandler((short)LobbyMessages.StartGame, (msg, room, account) =>
            {
                room.StartGame();
            });
        }
        private void PlayerLeavedHander(LobbyRoom<TAccountModel, TGame> room, TAccountModel player)
        {
            foreach (var p in room.Players)
            {
                if (p != player)
                {
                    _host.Send(new Message((short)LobbyMessages.PlayerLeaved, Mode.Reliable, DataStorage.CreateForWrite().Push(player.Connection.Token.ToInt())), p.Connection);
                }
                else
                {
                    _host.Send(new Message((short)LobbyMessages.SuccesfullyLeaved, Mode.Reliable), p.Connection);
                }
            }
        }
        private void PlayerJoinedHandler(LobbyRoom<TAccountModel, TGame> room, TAccountModel player)
        {
            foreach (var p in room.Players)
            {
                if (p != player)
                {
                    _host.Send(new Message((short)LobbyMessages.PlayerJoined, Mode.Reliable, DataStorage.CreateForWrite().Push(player.Connection.Token.ToInt())), p.Connection);
                }
                else
                {
                    _host.Send(new Message((short)LobbyMessages.SuccesfullyJoined, Mode.Reliable), p.Connection);
                }
            }
        }
        public override void AddHandler(short messageType, Action<Message, LobbyRoom<TAccountModel, TGame>, TAccountModel> roomHandler)
        {
            Action<Message, LobbyRoom<TAccountModel, TGame>, TAccountModel> newHandler = (msg, room, acc) => 
            {
                if (room._gameStarted)
                {
                    roomHandler.Invoke(msg, room, acc);
                }
                else
                {
                    Console.WriteLine("Стой, игра еще не началась...");
                }
            };

            base.AddHandler(messageType, newHandler);
        }
    }

    public class LobbyClient<TAccountModel> : IPlugin
        where TAccountModel : AccountModel, new()
    {
        Host _host;
        private Token _roomToken;
        public event Action<Token> RoomCreated;
        public event Action OnLeave;
        public event Action OnJoin;
        public event Action<Token> OnPlayerLeaved;
        public event Action<Token> OnPlayerJoined;
        public event Action OnGameStarted;
        public event Action<List<Token>> OnRoomInfoRecieved;
        public void JoinRoom(Token roomToken)
        {
            _host.Send(new Message((short)LobbyMessages.JoinRoom, Mode.Reliable, DataStorage.CreateForWrite().Push(roomToken.ToInt())));
        }
        public void GetRooms()
        {
            _host.Send(new Message((short)LobbyMessages.GetRooms, Mode.Reliable));
        }
        public void Send(Message msg)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(_roomToken.ToInt());
            ds.Push(msg.Body);
            msg.Body = ds.Serialize();
            _host.Send(msg);
        }
        public void CreateRoom()
        {
            _host.Send(new Message((short)LobbyMessages.CreateRoom, Mode.Reliable));
        }
        public void Bind(Host host)
        {
            _host = host;
            _host.AddHandler((short)LobbyMessages.RoomCreated, (m, c) =>
            {
                if (RoomCreated != null)
                {
                    var ds = DataStorage.CreateForRead(m.Body);
                    var roomToken = new Token(ds.ReadInt32());
                    RoomCreated.Invoke(roomToken);
                }
            });
            _host.AddHandler((short)LobbyMessages.PlayerLeaved, (m, c) =>
            {
                if (OnPlayerLeaved != null)
                {
                    var ds = DataStorage.CreateForRead(m.Body);
                    var playerToken = new Token(ds.ReadInt32());
                    OnPlayerLeaved.Invoke(playerToken);
                }
            });
            _host.AddHandler((short)LobbyMessages.SuccesfullyLeaved, (m, c) =>
            {
                if (OnLeave != null)
                {
                    OnLeave.Invoke();
                }
            });
            _host.AddHandler((short)LobbyMessages.PlayerJoined, (m, c) =>
            {
                if (OnPlayerJoined != null)
                {
                    var ds = DataStorage.CreateForRead(m.Body);
                    var playerToken = new Token(ds.ReadInt32());
                    OnPlayerJoined.Invoke(playerToken);
                }
            });
            _host.AddHandler((short)LobbyMessages.SuccesfullyJoined, (m, c) =>
            {
                if (OnJoin != null)
                {
                    OnJoin.Invoke();
                }
            });
            _host.AddHandler((short)LobbyMessages.GameStarted, (m, c) =>
            {
                if (OnGameStarted != null)
                {
                    var ds = DataStorage.CreateForRead(m.Body);
                    _roomToken = new Token(ds.ReadInt32());
                    OnGameStarted.Invoke();
                }
            });
            _host.AddHandler((short)LobbyMessages.GetRoomsResponse, (m, c) =>
            {
                if (OnRoomInfoRecieved != null)
                {
                    List<Token> roomTokens = new List<Token>();
                    roomTokens.FillDerialize(m.Body);
                    OnRoomInfoRecieved.Invoke(roomTokens);
                }
            });
        }
        public void StartGame()
        {
            _host.Send(new Message((short)LobbyMessages.StartGame, Mode.Reliable));
        }
    }
}
