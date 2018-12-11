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
            if (ShellPartsBinder.SelectedAccountModel == null) return;
            Lines.Clear();
            AccountName = ShellPartsBinder.SelectedAccountModel.Name;
            var isTag = !ShellPartsBinder.SelectedAccountModel.Is(_db.AccountsTree.First(a => a.Name == "Мои"));

            if (isTag) ShowTagBalance();
            else ShowTraffic(ShellPartsBinder.BalanceOrTraffic);
        }

        private void ShowTraffic(BalanceOrTraffic mode)
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountModel.IsFolder;

            var trafficCalculator = isLeaf
                ? (ITraffic)new TrafficOfAccountCalculator(_db, ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod)
                : new TrafficOfBranchCalculator(_db, ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod);

            trafficCalculator.Evaluate();

            foreach (var str in trafficCalculator.Report(mode)) Lines.Add(str);
            Total = trafficCalculator.Total;
        }

        private void ShowTagBalance()
        {

        }
    }
}
