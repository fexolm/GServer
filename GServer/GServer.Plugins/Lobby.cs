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
                    BackToQueue?.Invoke(this, _players.Where(p => p.Active).Select(p=>p.Account).ToArray());
                }
                else
                {
                    OnValidateSuccess?.Invoke(this, _players.Select(p => p.Account).ToArray());
                }
            }
        }
        public event Action<Lobby<TAccountModel>, TAccountModel[]> BackToQueue;
        public event Action<Lobby<TAccountModel>, TAccountModel[]> OnValidateSuccess;
    }
}
