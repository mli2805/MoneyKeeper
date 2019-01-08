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
        public int Mode { get; set; } // 1 - old. 2 - new.
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

        public void Initialize(int mode)
        {
            Mode = mode;
        }

        private async void Load()
        {
            _keeperDb.Bin = Mode == 1 ? await DbTexter.LoadAllFromOldTxt() : await DbTxtLoader.LoadAllFromNewTxt();
            DbLoaded = true;
            TryClose();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            TryClose();
        }
    }
}
