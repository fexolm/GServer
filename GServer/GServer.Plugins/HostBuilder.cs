using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Plugins
{
    public class Storage : IAuthStorage, IAccountStorage<AccountImpl>
    {
        private AuthUser[] _users;
        private AccountImpl[] _accounts;
        public Storage()
        {
            _users = new AuthUser[]
            {
                new AuthUser { AccountId = Guid.NewGuid(), Login = "player1", Password = "password1"},
                new AuthUser { AccountId = Guid.NewGuid(), Login = "player2", Password = "password2"},
                new AuthUser { AccountId = Guid.NewGuid(), Login = "artem", Password = "artem"},
                new AuthUser { AccountId = Guid.NewGuid(), Login = "test", Password = "test"},
            };
            _accounts = new AccountImpl[4];
            for (int i = 0; i < 4; i++)
            {
                _accounts[i] = new AccountImpl { AccountId = _users[i].AccountId };
            }
        }
        public IEnumerable<AuthUser> Users
        {
            get
            {
                return _users;
            }
        }
        public IEnumerable<AccountImpl> Accounts
        {
            get
            {
                return _accounts;
            }
        }
    }
    public class AccountImpl : AccountModel
    {
        #region unused
        public override void FillDeserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    public class PlayerQueueImpl : PlayerQueue<AccountImpl>
    {
        private List<AccountImpl> _players;
        public PlayerQueueImpl()
        {
            _players = new List<AccountImpl>();
        }
        #region unused
        public override void RemovePlayer(AccountImpl player)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override void AddPlayer(AccountImpl player)
        {
            lock (_players)
            {
                _players.Add(player);
                if (_players.Count == 2)
                {
                    OnGameFound.Invoke(_players.ToArray());
                    _players.Clear();
                }
            }
        }
    }


    public static class HostBuilder
    {

        public static Host CreateBaseServer<TGame>(int port, out RoomManager<TGame, AccountImpl> roomManager)
            where TGame : Game<AccountImpl>, new()
        {
            Host host = new Host(port);
            var st = new Storage();
            var auth = new Authorization(st);
            var account = new Account<AccountImpl>(auth, st);
            var matchmaking = new Matchmaking<AccountImpl>(account, new PlayerQueueImpl());
            roomManager = new RoomManager<TGame, AccountImpl>(matchmaking);
            return CreateServer<TGame, AccountImpl>(port, auth, account, matchmaking, roomManager);
        }
        public static Host CreateServer<TGame, TAccountModel>(
            int port,
            Authorization auth,
            Account<TAccountModel> account,
            Matchmaking<TAccountModel> matchmaking,
            RoomManager<TGame, TAccountModel> roomManager)
            where TGame : Game<TAccountModel>, new()
            where TAccountModel : AccountModel, new()
        {
            Host host = new Host(port);
            host.AddModule(auth);
            host.AddModule(account);
            host.AddModule(matchmaking);
            host.AddModule(roomManager);
            return host;
        }
        public static Host CreateClient<TAccountModel>(
            int port,
            out Authorization auth,
            out Matchmaking<TAccountModel> matchmaking)
            where TAccountModel : AccountModel, new()
        {
            Host host = new Host(port);
            auth = new Authorization();
            var account = new Account<TAccountModel>();
            matchmaking = new Matchmaking<TAccountModel>(account);
            host.AddModule(auth);
            host.AddModule(account);
            host.AddModule(matchmaking);
            return host;
        }
        public static Host CreateBaseClient(int port, out Authorization auth, out Matchmaking<AccountImpl> matchmaking)
        {
            return CreateClient<AccountImpl>(port, out auth, out matchmaking);
        }
    }
}
