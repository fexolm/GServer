namespace GServer.Plugins
{
    public abstract class GameSearcher<TAccountModel> : IPlugin
        where TAccountModel : AccountModel, new()
    {
        protected Host _host;
        protected Account<TAccountModel> _account;

        public GameSearcher(Account<TAccountModel> account) {
            _account = account;
        }

        public void Bind(Host host) {
            _host = host;
            InitializeHandlers();
        }

        protected abstract void InitializeHandlers();
    }
}