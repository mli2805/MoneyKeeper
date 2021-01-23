using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DbLoadingViewModel : Screen
    {
        private readonly KeeperDb _keeperDb;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;
        public bool DbLoaded { get; private set; }

        public DbLoadingViewModel(KeeperDb keeperDb)
        {
            _keeperDb = keeperDb;
            _cancellationToken = _cancellationTokenSource.Token;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Загрузка...";
            Task.Factory.StartNew(Load, _cancellationToken);
        }

        private async void Load()
        {
            _keeperDb.Bin = await DbTxtLoader.LoadAllFromNewTxt();
            if (_keeperDb.Bin == null)
            {
                DbLoaded = false;
                return;
            }
            DbTxtSaver.DeleteTxtFiles();
            DbLoaded = true;
            await DbSerializer.Serialize(_keeperDb.Bin);
            TryClose();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            TryClose();
        }
    }
}
