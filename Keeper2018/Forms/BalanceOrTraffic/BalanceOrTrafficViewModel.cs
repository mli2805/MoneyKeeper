using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class BalanceOrTrafficViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

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

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (value == _isPopupOpen) return;
                _isPopupOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public string PopupContent { get; set; } = "PopupContent";

        private List<KeyValuePair<DateTime, string>> _report;
        private ObservableCollection<string> _lines;
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

        private string _selectedLine;
        public string SelectedLine
        {
            get => _selectedLine;
            set
            {
                IsPopupOpen = false;
                _selectedLine = value;
            }
        }
        public string SelectedRowTooltip => SelectedLine;

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

        public BalanceOrTrafficViewModel(ShellPartsBinder shellPartsBinder, KeeperDataModel dataModel, 
            BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _dataModel = dataModel;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ShellPartsBinder = shellPartsBinder;
            ShellPartsBinder.PropertyChanged += ShellPartsBinder_PropertyChanged;
            Lines = new ObservableCollection<string>();
        }

        private void ShellPartsBinder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ShellPartsBinder.SelectedAccountItemModel == null) return;

            if (e.PropertyName == "SelectedAccountItemModel" || e.PropertyName == "JustToForceBalanceRecalculation"
                || e.PropertyName == "TranslatedPeriod" || e.PropertyName == "TranslatedDate")
            {
                Lines.Clear();
                AccountName = ShellPartsBinder.SelectedAccountItemModel.Name;
                var isTag = !ShellPartsBinder.SelectedAccountItemModel.Is(_dataModel.AccountsTree.First(a => a.Name == "Мои"));

                if (isTag) ShowTag();
                else ShowAccount(ShellPartsBinder.BalanceOrTraffic);
            }
        }

        private void ShowAccount(BalanceOrTraffic mode)
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountItemModel.Children.Any();

            var trafficCalculator = isLeaf
                ? (ITraffic)new TrafficOfAccountCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod)
                : new TrafficOfBranchCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod);

            trafficCalculator.EvaluateAccount();

            _report = trafficCalculator.Report(mode).ToList();
            foreach (var pair in _report) Lines.Add(pair.Value);
            Total = trafficCalculator.Total;
        }

        private void ShowTag()
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountItemModel.Children.Any();

            var trafficCalculator = isLeaf
                ? (ITraffic) new TrafficOfTagCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod):
                new TrafficOfTagBranchCalculator(_dataModel, ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod);

            trafficCalculator.EvaluateAccount();

            _report = trafficCalculator.Report(BalanceOrTraffic.Traffic).ToList();
            foreach (var pair in _report) Lines.Add(pair.Value);
            Total = trafficCalculator.Total;
        }

        public void ShowTransaction()
        {
            IsPopupOpen = false;

            var pair = _report.FirstOrDefault(p => p.Value == SelectedLine);
            if (pair.Key == DateTime.MinValue) return;

            var transaction = _dataModel.Transactions.Values.FirstOrDefault(t=>t.Timestamp == pair.Key);
            if (transaction == null) return;

            InitializePopupContent(transaction);
            IsPopupOpen = true;
        }

        public ObservableCollection<string> PopupLabels { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> PopupValues { get; set; } = new ObservableCollection<string>();
        private void InitializePopupContent(TransactionModel tranModel)
        {
            PopupLabels.Clear();
            PopupValues.Clear();

            PopupLabels.Add("Timestamp: ");
            PopupValues.Add(tranModel.Timestamp.ToString("dd-MM-yyyy HH:mm"));

            if (tranModel.Operation == OperationType.Перенос || tranModel.Operation == OperationType.Обмен)
            {
                PopupLabels.Add($"{tranModel.Operation} с:");
                PopupLabels.Add(" на:");
                PopupValues.Add(tranModel.MyAccount.Name);
                PopupValues.Add(tranModel.MySecondAccount.Name);
            }

            if (tranModel.Operation == OperationType.Обмен)
            {
                PopupLabels.Add("");
                PopupValues.Add($" ({_balanceDuringTransactionHinter.GetExchangeRate(tranModel)})");
            }

            PopupLabels.Add("Amount: ");
            var amount = _dataModel.AmountInUsdWithRate(tranModel.Timestamp, 
                tranModel.Currency, tranModel.Amount, out decimal rate);

            if (tranModel.Currency != CurrencyCode.USD)
                amount += $" (rate {rate:0.####})";
            PopupValues.Add(amount);

            if (tranModel.Operation == OperationType.Обмен)
            {
                PopupLabels.Add("Amount in return: ");
                var amountInReturn = _dataModel.AmountInUsdWithRate(tranModel.Timestamp, 
                    tranModel.CurrencyInReturn, tranModel.AmountInReturn, out decimal rateInReturn);

                if (tranModel.CurrencyInReturn != CurrencyCode.USD)
                    amountInReturn += $" (rate {rateInReturn:0.####})";
                PopupValues.Add(amountInReturn);
            }

            var flag = true;
            foreach (var accountModel in tranModel.Tags)
            {
                if (flag)
                {
                    PopupLabels.Add("Tags:");
                    flag = false;
                }
                else
                {
                    PopupLabels.Add("");
                }
                PopupValues.Add(accountModel.Name);
            }

            
            PopupLabels.Add("Comment: ");
            PopupValues.Add($"{tranModel.Comment}");
        }

    }
}
