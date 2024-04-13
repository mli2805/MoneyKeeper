using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class BalanceOrTrafficViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

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

        public RangeObservableCollection<ListLine> ColoredLines { get; set; }

        private ListLine _selectedColoredLine;
        public ListLine SelectedColoredLine 
        {
            get => _selectedColoredLine;
            set
            {
                if (Equals(value, _selectedColoredLine)) return;
                _selectedColoredLine = value;
                NotifyOfPropertyChange();
            }
        }

        private string _total;
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

        public BalanceOrTrafficViewModel(ShellPartsBinder shellPartsBinder, KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
            ShellPartsBinder = shellPartsBinder;
            ShellPartsBinder.PropertyChanged += ShellPartsBinder_PropertyChanged;
            ColoredLines = new RangeObservableCollection<ListLine>();
        }

        private void ShellPartsBinder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ShellPartsBinder.SelectedAccountItemModel == null) return;

            if (e.PropertyName == "SelectedAccountItemModel" || e.PropertyName == "JustToForceBalanceRecalculation"
                || e.PropertyName == "TranslatedPeriod" || e.PropertyName == "TranslatedDate")
            {
                ColoredLines.Clear();
                AccountName = ShellPartsBinder.SelectedAccountItemModel.Name;
                var isMine = ShellPartsBinder.SelectedAccountItemModel.IsMyAccount();

                if (isMine)
                {
                    ShowAccount(ShellPartsBinder.BalanceOrTraffic);
                }
                else
                {
                    ShowTag();
                }
            }
        }

        private void ShowAccount(BalanceOrTraffic mode)
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountItemModel.Children.Any();

            var trafficCalculator = isLeaf
                ? (ITraffic)new TrafficOfAccountCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel,
                    ShellPartsBinder.SelectedPeriod)
                : new TrafficOfAccountBranchCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel,
                    ShellPartsBinder.SelectedPeriod);

            trafficCalculator.EvaluateAccount();

            ColoredLines.AddRange(trafficCalculator.ColoredReport(mode).Select(p=>p.Value));

            Total = trafficCalculator.Total;
        }

        private void ShowTag()
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountItemModel.Children.Any();

            var trafficCalculator = isLeaf
                ? (ITraffic) new TrafficOfTagCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel,
                    ShellPartsBinder.SelectedPeriod):
                new TrafficOfTagBranchCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel,
                    ShellPartsBinder.SelectedPeriod);

            trafficCalculator.EvaluateAccount();
            ColoredLines.AddRange(
                trafficCalculator.ColoredReport(BalanceOrTraffic.Balance).Select(p=>p.Value));

            Total = trafficCalculator.Total;
        }
    }
}
