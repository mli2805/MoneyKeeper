using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

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
            var result = await DbTxtLoader.LoadAllFromNewTxt();
            if (!result.IsSuccess)
            {
                _keeperDb.Bin = null;
                DbLoaded = false;
                MessageBox.Show(result.Exception.Message);
                return;
            }

            _keeperDb.Bin = (KeeperBin) result.Payload;
          
            var result2 = DbTxtSaver.DeleteTxtFiles();
            if (!result2.IsSuccess)
            {
                MessageBox.Show(result2.Exception.Message);
            }
            DbLoaded = true;

            var result3 = await BinSerializer.Serialize(_keeperDb.Bin);
            if (!result3.IsSuccess)
            {
                MessageBox.Show(result3.Exception.Message);
            }
            TryClose();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            TryClose();
        }
    }
}
