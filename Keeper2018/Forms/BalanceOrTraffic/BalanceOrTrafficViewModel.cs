using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class BalanceOrTrafficViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _db;

        private TranTooltipViewModel _vm;
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

        private List<KeyValuePair<DateTime, string>> _report;
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

        public string SelectedLine { get; set; }
        public string SelectedRowTooltip => SelectedLine;
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

        public BalanceOrTrafficViewModel(IWindowManager windowManager, ShellPartsBinder shellPartsBinder, KeeperDb db)
        {
            _windowManager = windowManager;
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

            if (isTag) ShowTag();
            else ShowAccount(ShellPartsBinder.BalanceOrTraffic);
        }

        private void ShowAccount(BalanceOrTraffic mode)
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountModel.IsFolder;

            var trafficCalculator = isLeaf
                ? (ITraffic)new TrafficOfAccountCalculator(_db, ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod)
                : new TrafficOfBranchCalculator(_db, ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod);

            trafficCalculator.Evaluate();

            _report = trafficCalculator.Report(mode).ToList();
            foreach (var pair in _report) Lines.Add(pair.Value);
            Total = trafficCalculator.Total;
        }

        private void ShowTag()
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountModel.IsFolder;

            var trafficCalculator = isLeaf
                ? (ITraffic) new TrafficOfTagCalculator(_db, ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod):
                new TrafficOfTagBranchCalculator(_db, ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod);

            trafficCalculator.Evaluate();

            _report = trafficCalculator.Report(BalanceOrTraffic.Traffic).ToList();
            foreach (var pair in _report) Lines.Add(pair.Value);
            Total = trafficCalculator.Total;
        }

        public void ShowTransaction()
        {
            var pair = _report.FirstOrDefault(p => p.Value == SelectedLine);
            if (pair.Key == DateTime.MinValue) return;

            var transaction = _db.Bin.Transactions.Values.FirstOrDefault(t=>t.Timestamp == pair.Key);
            if (transaction == null) return;

            _vm?.TryClose();
            _vm = new TranTooltipViewModel(_db, transaction);
            
            _windowManager.ShowWindow(_vm);
        }
    }
}
