using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class BalanceOrTrafficViewModel : Screen
    {
        private readonly KeeperDb _db;
        private ShellPartsBinder ShellPartsBinder { get; }

        private string _accountName;
        public string AccountName
        {
            get => _accountName;
            set
            {
                if (value == _accountName) return;
                _accountName = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<string> _lines;
        private string _total;

        public ObservableCollection<string> Lines
        {
            get => _lines;
            set
            {
                if (Equals(value, _lines)) return;
                _lines = value;
                NotifyOfPropertyChange();
            }
        }

        public string Total
        {
            get => _total;
            set
            {
                if (value == _total) return;
                _total = value;
                NotifyOfPropertyChange();
            }
        }

        public BalanceOrTrafficViewModel(ShellPartsBinder shellPartsBinder, KeeperDb db)
        {
            _db = db;
            ShellPartsBinder = shellPartsBinder;
            ShellPartsBinder.PropertyChanged += ShellPartsBinder_PropertyChanged;
            Lines = new ObservableCollection<string>();
        }

        private void ShellPartsBinder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AccountName = ShellPartsBinder.SelectedAccountModel.Name;
            Lines.Clear();

            if (ShellPartsBinder.BalanceOrTraffic == BalanceOrTraffic.Balance)
                ShowBalance();
            else
                ShowTraffic();
        }

        private void ShowTraffic()
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountModel.IsFolder;

            var traffic = isLeaf
                ? (ITraffic) new TrafficOfLeaf(ShellPartsBinder.SelectedAccountModel, _db)
                : new TrafficOfFolder(ShellPartsBinder.SelectedAccountModel);

            foreach (var tran in _db.TransactionModels.Where(t => ShellPartsBinder.SelectedPeriod.Includes(t.Timestamp)))
                traffic.RegisterTran(tran);

            foreach (var str in traffic.Report()) Lines.Add(str);
            //      Total = traffic.Total;
        }

        private void ShowBalance()
        {
            var balance = new BalanceOfAccount(ShellPartsBinder.SelectedAccountModel, _db);
            foreach (var tran in _db.TransactionModels.Where(t => ShellPartsBinder.SelectedPeriod.Includes(t.Timestamp)))
                balance.RegisterTran(tran);
            foreach (var str in balance.Report(ShellPartsBinder.SelectedPeriod.FinishMoment)) Lines.Add(str);
            Total = balance.Total;
        }
    }
}
