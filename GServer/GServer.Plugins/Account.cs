using System;
using System.Collections.Generic;
using System.Linq;
using GServer.Containers;

namespace GServer.Plugins
{
    public abstract class AccountModel : ISerializable, IDeserializable
    {
        public Guid AccountId { get; set; }
        public Connection Connection { get; set; }
        public Token RoomToken { get; set; }
        public abstract void FillDeserialize(byte[] buffer);
        public abstract byte[] Serialize();
    }

    public interface IAccountStorage<TModel>
        where TModel : AccountModel
    {
        IEnumerable<TModel> Accounts { get; }
    }

    public class Account<TModel> : IPlugin
        where TModel : AccountModel, new()
    {
        enum AccountMessage
        {
            InfoRequest = 2000,
            InfoResponse = 2001,
        }
        private readonly IAuthorization _auth;
        private readonly IDictionary<Token, TModel> _accounts;
        private readonly IAccountStorage<TModel> _storage;
        private Host _host;
        public Account(IAuthorization auth, IAccountStorage<TModel> storage)
        {
            _auth = auth;
            _accounts = new Dictionary<Token, TModel>();
            _auth.OnAccountLogin += AccountLoginHandler;
            _storage = storage;
        }
        public Account() { }
        private void AccountLoginHandler(Connection connection, Guid accountId)
        {
            lock (_accounts)
            {
                if (!_accounts.ContainsKey(connection.Token))
                {
                    var account = _storage.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                    account.Connection = connection;
                    _accounts.Add(connection.Token, account);
                    connection.Disconnected += () =>
                    {
                        _accounts.Remove(connection.Token);
                    };
                }
            }
        }
        public void Bind(Host host)
        {
            _host = host;
            host.AddHandler((short)AccountMessage.InfoRequest, InfoRequestHandler);
            host.AddHandler((short)AccountMessage.InfoResponse, InfoResponseHandler);
        }
        private void InfoResponseHandler(Message msg, Connection con)
        {
            TModel account = new TModel();
            account.FillDeserialize(msg.Body);
           if (OnAccountInfoReceived!=null) OnAccountInfoReceived.Invoke(account);
        }
        private void InfoRequestHandler(Message msg, Connection con)
        {
            lock (_accounts)
            {
                if (_accounts.ContainsKey(con.Token))
                {
                    _host.Send(new Message((short)AccountMessage.InfoResponse, Mode.None, _accounts[con.Token].Serialize()), con);
                }
            }
        }
        public void GetInfo()
        {
            _host.Send(new Message((short)AccountMessage.InfoRequest, Mode.None));
        }
        public event Action<TModel> OnAccountInfoReceived;
        public bool IsAuthorized(Token token)
        {
            return _accounts.ContainsKey(token);
        }
        public TModel GetAccount(Token token)
        {
            lock (_accounts)
            {
                if (_accounts.ContainsKey(token))
                {
                    return _accounts[token];
                }
            }
            return null;
        }
        public void AddHandler(short messageType, Action<Message, TModel> accountHandler)
        {
            _host.AddHandler(messageType, (m, c) =>
            {
                var acc = GetAccount(c.Token);
                accountHandler.Invoke(m, acc);
            });
        }
    }
}
