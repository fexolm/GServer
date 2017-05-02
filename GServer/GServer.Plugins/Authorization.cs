﻿using System;
using System.Collections.Generic;
using System.Linq;
using GServer.Containers;
namespace GServer.Plugins
{
    public class AuthUser
    {
        public Guid AccountId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Online = false;
    }

    public interface IAuthStorage
    {
        IEnumerable<AuthUser> Users { get; }
    }

    public class AuthSession
    {
        public AuthUser User;
        public int Num;
    }

    public class Authorization : IPlugin, IAuthorization
    {
        private enum AuthMType
        {
            Chalange = 1000,
            ChalangeFailed = 1001,
            ChalangeSuccess = 1002,
            PwdHash = 1003,
            AuthSuccess = 1004,
            AuthFailed = 1005,
        }
        private readonly IAuthStorage _storage;
        private readonly bool _isClient;
        private Host _host;
        private readonly IDictionary<Token, AuthSession> _sessions;
        private string Login;
        private string Password;

        public Authorization(IAuthStorage storage)
        {
            _storage = storage;
            _isClient = false;
            _sessions = new Dictionary<Token, AuthSession>();
        }
        public Authorization()
        {
            _isClient = true;
        }
        public void Bind(Host host)
        {
            _host = host;
            if (_isClient)
            {
                host.AddHandler((short)AuthMType.ChalangeSuccess, SendPwdHash);
                host.AddHandler((short)AuthMType.ChalangeFailed, (m, e) => OnAuthFailed?.Invoke("Логин не найден"));
                host.AddHandler((short)AuthMType.AuthFailed, (m, e) => OnAuthFailed?.Invoke("Пароль не подошел"));
                host.AddHandler((short)AuthMType.AuthSuccess, (m, e) => OnAuthSuccess?.Invoke());
            }
            else
            {
                host.AddHandler((short)AuthMType.Chalange, ChalangeHandler);
                host.AddHandler((short)AuthMType.PwdHash, CheckHash);
            }
        }
        private void ChalangeHandler(Message m, Connection c)
        {
            Console.WriteLine("auth response from {0}", c.EndPoint.ToString());
            var login = DataStorage.CreateForRead(m.Body).ReadString();
            var user = _storage.Users.FirstOrDefault(u => u.Login == login);
            if (user == null || user.Online)
            {
                _host.Send(new Message((short)AuthMType.ChalangeFailed, Mode.Reliable), c);
            }
            else
            {
                int num = _host.Rnd.Next(1000000, int.MaxValue);
                lock (_sessions)
                {
                    var session = new AuthSession();
                    session.User = user;
                    session.Num = num;
                    _sessions.Add(c.Token, session);
                }
                _host.Send(new Message((short)AuthMType.ChalangeSuccess, Mode.Reliable, DataStorage.CreateForWrite().Push(num).Serialize()), c);
            }
        }
        private void SendPwdHash(Message m, Connection c)
        {
            var num = DataStorage.CreateForRead(m.Body).ReadInt32();
            var hash = Password.GetHashCode();
            _host.Send(new Message((short)AuthMType.PwdHash, Mode.Reliable, DataStorage.CreateForWrite().Push(num ^ hash).Serialize()));
        }
        private void CheckHash(Message m, Connection c)
        {
            var hash = DataStorage.CreateForRead(m.Body).ReadInt32();
            lock (_sessions)
            {
                if (_sessions.ContainsKey(c.Token))
                {
                    var session = _sessions[c.Token];
                    if (!session.User.Online)
                    {
                        session.User.Online = true;
                        _host.Send(new Message((short)AuthMType.AuthSuccess, Mode.Reliable), c);
                        c.Disconnected += () => session.User.Online = false;
                        OnAccountLogin?.Invoke(c, session.User.AccountId);
                    }
                    else
                    {
                        _host.Send(new Message((short)AuthMType.AuthFailed, Mode.Reliable), c);
                    }
                    _sessions.Remove(c.Token);
                }
                else
                {
                    _host.Send(new Message((short)AuthMType.AuthFailed, Mode.Reliable), c);
                }
            }
        }
        public void BeginAuth(string login, string pass)
        {
            Login = login;
            Password = pass;
            _host.Send(new Message((short)AuthMType.Chalange, Mode.Reliable, DataStorage.CreateForWrite().Push(login).Serialize()));
        }
        public event Action<Connection, Guid> OnAccountLogin;
        public event Action OnAuthSuccess;
        public event Action<string> OnAuthFailed;
    }

    public interface IAuthorization
    {
        event Action<Connection, Guid> OnAccountLogin;
        event Action OnAuthSuccess;
        event Action<string> OnAuthFailed;
        void BeginAuth(string login, string pass);
    }
}
