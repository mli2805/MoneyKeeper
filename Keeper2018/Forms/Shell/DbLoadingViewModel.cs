using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DbLoadingViewModel : Screen
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;
        public LibResult LoadResult { get; set; }

        public DbLoadingViewModel()
        {
            _cancellationToken = _cancellationTokenSource.Token;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Загрузка...";
            Task.Factory.StartNew(Load, _cancellationToken);
        }

        private async Task Load()
        {
            // await Task.Delay(0);
            LoadResult = TxtLoader.LoadAllFromNewTxt(PathFactory.GetBackupPath());
            if (!LoadResult.IsSuccess)
            {
                MessageBox.Show(LoadResult.Exception.Message);
                return;
            }

            var delResult = TxtSaver.DeleteTxtFiles();
            if (!delResult.IsSuccess)
            {
                MessageBox.Show(delResult.Exception.Message);
            }

            var serializeResult = await BinSerializer.Serialize((KeeperBin)LoadResult.Payload);
            if (!serializeResult.IsSuccess)
            {
                MessageBox.Show(serializeResult.Exception.Message);
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
