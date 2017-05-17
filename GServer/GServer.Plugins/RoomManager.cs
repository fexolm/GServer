using GServer.Containers;
using GServer.Plugins.Matchmaking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GServer.Plugins
{
    public abstract class RoomManager<TGame, TAccountModel, TRoom> : IPlugin
        where TAccountModel : AccountModel, new()
        where TGame : Game<TAccountModel>, new()
        where TRoom : Room<TAccountModel, TGame>
    {
        public RoomManager()
        {
            _rooms = new Dictionary<Token, TRoom>();
        }
        protected IDictionary<Token, TRoom> _rooms;
        protected Host _host;
        public void Bind(Host host)
        {
            _host = host;
            InitializeHandlers();
        }
        public virtual void InitializeHandlers() { }
        public virtual void AddHandler(short messageType, Action<Message, TRoom, TAccountModel> roomHandler)
        {
            _host.AddHandler(messageType, (m, c) =>
            {
                var ds = DataStorage.CreateForRead(m.Body);
                var token = new Token(ds.ReadInt32());
                m.Body = ds.ReadToEnd();
                lock (_rooms)
                {
                    if (_rooms.ContainsKey(token))
                    {
                        var room = _rooms[token];
                        roomHandler.Invoke(m, room, room.Players.FirstOrDefault(p => p.Connection.Token == c.Token));
                    }
                }
            });
        }
    }
}
