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
            return GetChildBalanceLines();
        }

        private IEnumerable<string> GetChildBalanceLines()
        {
            var childrenBalances = new GroupOfBalances();
            foreach (var tran in _db.TransactionModels.Where(t=>ShellPartsBinder.SelectedPeriod.IsIn(t.Timestamp)))
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
                        if (myAcc2 != null && tran.CurrencyInReturn != null)
                            childrenBalances.Add(myAcc2, tran.Currency, tran.Amount);
                        break;
                    }  case OperationType.Обмен:
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
            yield return null;
        }
    }

    public class TrafficPair
    {
        public decimal Plus;
        public decimal Minus;
    }

    public class Traffic
    {
        private readonly Dictionary<CurrencyCode, TrafficPair> _money;
        public void Add(CurrencyCode currency, decimal amount)
        {
            if (_money.ContainsKey(currency)) _money[currency].Plus = _money[currency].Plus + amount; 
            else _money.Add(currency, new TrafficPair(){Plus = amount});
        }

        public void Sub(CurrencyCode currency, decimal amount)
        {
            if (_money.ContainsKey(currency)) _money[currency].Minus = _money[currency].Minus + amount; 
            else _money.Add(currency, new TrafficPair(){Minus = amount});
        }
    }

    public class Balance
    {
        public readonly Dictionary<CurrencyCode, decimal> Currencies;

        public Balance() { Currencies = new Dictionary<CurrencyCode, decimal>(); }

        public Balance(CurrencyCode currency, decimal amount)
        {
            Currencies = new Dictionary<CurrencyCode, decimal>() { { currency, amount } };
        }
        public void Add(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency] = Currencies[currency] + amount; else Currencies.Add(currency, amount);
        }

        public void Sub(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency] = Currencies[currency] - amount; else Currencies.Add(currency, -amount);
        }

        public void AddBalance(Balance balance) 
        {
            foreach (var currency in balance.Currencies) { Add(currency.Key, currency.Value); }
        }

    }

    public class GroupOfBalances
    {
        private Dictionary<AccountModel, Balance> _children = new Dictionary<AccountModel, Balance>();

        public void Add(AccountModel child, CurrencyCode currency, decimal amount)
        {
            if (_children.ContainsKey(child)) _children[child].Add(currency, amount);
            else
                _children.Add(child, new Balance(currency, amount));
        }

        public void Sub(AccountModel child, CurrencyCode currency, decimal amount)
        {
            if (_children.ContainsKey(child)) _children[child].Sub(currency, amount);
            else
                _children.Add(child, new Balance(currency, -amount));
        }

        private Balance Sum()
        {
            var result = new Balance();
            foreach (var child in _children)
            {
                result.AddBalance(child.Value);
            }
            return result;
        }


        public IEnumerable<string> Report(string parentName)
        {
            foreach (var currency in Sum().Currencies) if (currency.Value > 0) yield return $"{currency.Key} {currency.Value}";

            if (_children.Count == 0 || _children.Count == 1 && _children.First().Key.Name == parentName) yield break;

            foreach (var pair in _children)
            {
                if (pair.Value.Currencies.Any(c=>c.Value != 0))
                {
                    yield return "      " + pair.Key.Name;
                    foreach (var currency in pair.Value.Currencies) 
                        { if (currency.Value > 0) yield return $"   {currency.Key} {currency.Value}"; }
                }
             }
        }
    }
}
