using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class BalanceOrTrafficViewModel : Screen
    {
        private readonly KeeperDb _db;
        public ShellPartsBinder ShellPartsBinder { get; }

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
        public ObservableCollection<string> Lines
        {
            get { return _lines; }
            set
            {
                if (Equals(value, _lines)) return;
                _lines = value;
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
            Show();
        }

        private void Show()
        {
            AccountName = ShellPartsBinder.SelectedAccountModel.Name;
            Lines.Clear();
            foreach (var line in GetLines()) Lines.Add(line);
        }

        private IEnumerable<string> GetLines()
        {
            return ShellPartsBinder.BalanceOrTraffic == BalanceOrTraffic.Balance
                ? GetBalanceLines()
                : GetTrafficLines();
        }

        // + расшифровка одного уровня ниже
        private IEnumerable<string> GetBalanceLines()
        {
            var childrenBalances = new GroupOfBalances();
            foreach (var tran in _db.TransactionModels.Where(t => ShellPartsBinder.SelectedPeriod.Includes(t.Timestamp)))
            {
                switch (tran.Operation)
                {
                    case OperationType.Доход:
                        {
                            var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                            if (myAcc != null)
                                childrenBalances.Add(myAcc, tran.Currency, tran.Amount);
                            break;
                        }
                    case OperationType.Расход:
                        {
                            var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                            if (myAcc != null)
                                childrenBalances.Sub(myAcc, tran.Currency, tran.Amount);
                            break;
                        }
                    case OperationType.Перенос:
                        {
                            var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                            if (myAcc != null)
                                childrenBalances.Sub(myAcc, tran.Currency, tran.Amount);
                            var myAcc2 = tran.MySecondAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                            if (myAcc2 != null)
                                childrenBalances.Add(myAcc2, tran.Currency, tran.Amount);
                            break;
                        }
                    case OperationType.Обмен:
                        {
                            var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                            if (myAcc != null)
                                childrenBalances.Sub(myAcc, tran.Currency, tran.Amount);
                            var myAcc2 = tran.MySecondAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                            if (myAcc2 != null && tran.CurrencyInReturn != null)
                                childrenBalances.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                            break;
                        }
                }
            }
            return childrenBalances.Report(ShellPartsBinder.SelectedAccountModel.Name);
        }

        private IEnumerable<string> GetTrafficLines()
        {
            var isLeaf = !ShellPartsBinder.SelectedAccountModel.IsFolder;

            var traffic = isLeaf
                ? (ITraffic)new TrafficOfLeaf(ShellPartsBinder.SelectedAccountModel, _db)
                : new TrafficOfFolder(ShellPartsBinder.SelectedAccountModel);

            foreach (var tran in _db.TransactionModels.Where(t => ShellPartsBinder.SelectedPeriod.Includes(t.Timestamp)))
                traffic.RegisterTran(tran);

            return traffic.Report();
        }
      
    }
}
