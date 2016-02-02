using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public abstract class TrBase : PropertyChangedBase
    {
        private DateTime _timestamp;
        private Account _myAccount;
        private decimal _amount;
        private CurrencyCodes _currency;
        private List<Account> _tags;
        private string _comment;

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange();
            }
        }
        public Account MyAccount
        {
            get { return _myAccount; }
            set
            {
                if (Equals(value, _myAccount)) return;
                _myAccount = value;
                NotifyOfPropertyChange();
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value.Equals(_amount)) return;
                _amount = value;
                NotifyOfPropertyChange();
            }
        }
        public CurrencyCodes Currency
        {
            get { return _currency; }
            set
            {
                if (value == _currency) return;
                _currency = value;
                NotifyOfPropertyChange();
            }
        }
        public List<Account> Tags
        {
            get { return _tags; }
            set
            {
                if (Equals(value, _tags)) return;
                _tags = value;
                NotifyOfPropertyChange();
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public abstract Brush TransactionFontColor { get; }
        public Brush DayBackgroundColor
        {
            get
            {
                var daysFrom = Timestamp.Date - new DateTime(1972, 5, 28);
                if (daysFrom.Days % 4 == 0) return Brushes.Cornsilk;
                if (daysFrom.Days % 4 == 1) return new SolidColorBrush(Color.FromRgb(240, 255, 240));
                if (daysFrom.Days % 4 == 2) return Brushes.GhostWhite;
                return Brushes.Azure;
            }
        }

        public string AccountForDatagrid => $"{MyAccount}";
        public string AmountForDatagrid => ShowAmount(_amount, _currency);

        protected string ShowAmount(decimal amount, CurrencyCodes currency)
        {
            return currency == CurrencyCodes.BYR
                ? $"{amount:0,0} {currency.ToString().ToLower()}"
                : $"{amount:0,0.00} {currency.ToString().ToLower()}";
        }

        public string TagsForDatagrid => ShowTags();

        private string ShowTags()
        {
            string result = "";
            if (Tags.Count > 0) result = Tags[0].ToString();
            for (int i = 1; i < Tags.Count; i++)
                result = result + ";  " + Tags[i].ToString();
            return result;
        }

        public abstract int SignForAmount(Account account);
        public abstract MoneyBag AmountForAccount(Account account);

    }
}
