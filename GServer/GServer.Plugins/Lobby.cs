using System;
using System.Collections.Generic;
using System.Collections;
using GServer.Containers;
using System.Linq;
using System.Text;

namespace GServer.Plugins
{

    class Lobby<TAccountModel>
        where TAccountModel : AccountModel
    {
        private class __PlayerStatus
        {
            public TAccountModel Account;
            public bool Active;
            public __PlayerStatus(TAccountModel account)
            {
                Account = account;
                Active = false;
            }
        }

        private __PlayerStatus[] _players;
        private Host _host;
        private int _ticks = 0;
        public Lobby(TAccountModel[] players, Host host)
        {
            Console.WriteLine("Lobby created");
            _host = host;
            _players = players.Select(p => new __PlayerStatus(p)).ToArray();
            foreach (var player in _players)
            {
                _host.Send(new Message((short)MatchmakingMessages.ValidateActivity, Mode.Reliable),
                    player.Account.Connection);

                player.Account.Connection.AddHandler((short)MatchmakingMessages.ValidateActivitySuccess, (m) =>
                {
                    player.Active = true;
                });
            }
            _host.OnTick += Tick;
        }

        private void Tick()
        {
            _ticks++;
            if (_ticks > 200)
            {
                _host.OnTick -= Tick;
                if (_players.FirstOrDefault(p => !p.Active) != null)
                {
                    BackToQueue?.Invoke(this, _players.Where(p => p.Active).Select(p => p.Account).ToArray());
                    foreach (var p in _players.Where(p => !p.Active))
                    {
                        _host.ForceDisconnect(p.Account.Connection);
                        Console.WriteLine("disconnecting {0}", p.Account.Connection.EndPoint.ToString());
                    }
                }
                else
                {
                    OnValidateSuccess?.Invoke(this, _players.Select(p => p.Account).ToArray());
                }
                foreach (var p in _players)
                {
                    p.Account.Connection.RemoveHandler((short)MatchmakingMessages.ValidateActivitySuccess);
                }
            }
        }
        public event Action<Lobby<TAccountModel>, TAccountModel[]> BackToQueue;
        public event Action<Lobby<TAccountModel>, TAccountModel[]> OnValidateSuccess;
    }
}
