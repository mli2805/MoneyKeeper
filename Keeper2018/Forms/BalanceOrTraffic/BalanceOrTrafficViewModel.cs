using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }

        private void ShellPartsBinder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Lines.Clear();
            foreach (var line in GetLines())
            {
                Lines.Add(line);
            }
        }

        private IEnumerable<string> GetLines()
        {
            return ShellPartsBinder.BalanceOrTraffic == BalanceOrTraffic.Balance
                ? GetBalanceLines()
                : GetTrafficLines();
        }

        // с расшифровкой одного уровня
        private IEnumerable<string> GetBalanceLines()
        {
            return null;
        }

        private IEnumerable<string> GetChildBalanceLines()
        {
            var childrenBalances = new ChildrenBalances();
            foreach (var tran in _db.TransactionModels)
            {
                switch (tran.Operation)
                {
                    case OperationType.Доход:
                    {
                        var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                        if (myAcc != null) childrenBalances.Add(myAcc, (CurrencyCode)tran.Currency, tran.Amount);
                        break;
                    }
                    case OperationType.Расход:
                    {
                        var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                        if (myAcc != null) childrenBalances.Sub(myAcc, (CurrencyCode)tran.Currency, tran.Amount);
                        break;
                    }
                    case OperationType.Перенос:
                    case OperationType.Обмен:
                    {
                        var myAcc = tran.MyAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                        if (myAcc != null) childrenBalances.Sub(myAcc, (CurrencyCode)tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(ShellPartsBinder.SelectedAccountModel);
                        if (myAcc2 != null) childrenBalances.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);

                        break;
                    }
                }
            }

            return null;
        }

        private IEnumerable<string> GetTrafficLines()
        {
            return null;
        }
    }

    public class Balance
    {
        private readonly Dictionary<CurrencyCode, double> _money;

        public Balance(CurrencyCode currency, double amount)
        {
            _money = new Dictionary<CurrencyCode, double>() { { currency, amount } };
        }
        public void Add(CurrencyCode currency, double amount)
        {
            if (_money.ContainsKey(currency)) _money[currency] = _money[currency] + amount; else _money.Add(currency, amount);
        }

        public void Sub(CurrencyCode currency, double amount)
        {
            if (_money.ContainsKey(currency)) _money[currency] = _money[currency] - amount; else _money.Add(currency, -amount);
        }
    }

    public class ChildrenBalances
    {
        private Dictionary<AccountModel, Balance> _children = new Dictionary<AccountModel, Balance>();

        public void Add(AccountModel child, CurrencyCode currency, double amount)
        {
            if (_children.ContainsKey(child)) _children[child].Add(currency, amount);
            else
                _children.Add(child, new Balance(currency, amount));
        }

        public void Sub(AccountModel child, CurrencyCode currency, double amount)
        {
            if (_children.ContainsKey(child)) _children[child].Sub(currency, amount);
            else
                _children.Add(child, new Balance(currency, -amount));
        }
    }
}
