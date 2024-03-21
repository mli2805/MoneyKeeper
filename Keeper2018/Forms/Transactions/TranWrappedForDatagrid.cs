using System;
using System.Windows.Media;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TranWrappedForDataGrid : PropertyChangedBase
    {
        private TransactionModel _tran;
        public TransactionModel Tran
        {
            get { return _tran; }
            set
            {
                if (Equals(value, _tran)) return;
                _tran = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(DayBackgroundColor));
                NotifyOfPropertyChange(nameof(TransactionFontColor));
                NotifyOfPropertyChange(nameof(AccountForDataGrid));
                NotifyOfPropertyChange(nameof(AmountForDataGrid));
                NotifyOfPropertyChange(nameof(TagsForDataGrid));
            }
        }

        public TranWrappedForDataGrid(TransactionModel tran)
        {
            Tran = tran;
        }

        public string AccountForDataGrid => GetAccountForDataGrid();
        public string AmountForDataGrid => GetAmountForDataGrid();
        public string TagsForDataGrid => GetTagsForDataGrid();

        private string GetAccountForDataGrid()
        {
            return IsOneAccountTransaction() ? Tran.MyAccount.Name : $"{Tran.MyAccount.Name} ->\n  {Tran.MySecondAccount.Name}";
        }
        private string GetAmountForDataGrid()
        {
            return IsOneAmountTransaction()
                ? ShowAmount(Tran.Amount, Tran.Currency)
                : ShowAmount(Tran.Amount, Tran.Currency) + " ->\n    " + ShowAmount(Tran.AmountInReturn, Tran.CurrencyInReturn);
        }
        private string ShowAmount(decimal amount, CurrencyCode? currency)
        {
            return currency == CurrencyCode.BYR
                ? $" {amount:#,0} {currency.ToString().ToLower()}"
                : $" {amount:#,0.00} {currency.ToString().ToLower()}";
        }
        private string GetTagsForDataGrid()
        {
            string result = "";
            if (Tran.Tags.Count > 0) result = Tran.Tags[0].ToString();
            for (int i = 1; i < Tran.Tags.Count; i++)
                result = result + ";  " + Tran.Tags[i];
            return result;
        }
        private bool IsOneAccountTransaction()
        {
            return Tran.Operation == OperationType.Доход ||
                   Tran.Operation == OperationType.Расход ||
                   (Tran.Operation == OperationType.Обмен && Equals(Tran.MyAccount, Tran.MySecondAccount));
        }
        private bool IsOneAmountTransaction()
        {
            return Tran.Operation == OperationType.Доход ||
                   Tran.Operation == OperationType.Расход ||
                   Tran.Operation == OperationType.Перенос;
        }

        #region // цвет шрифта и фона для отображения транзакции
        public Brush DayBackgroundColor
        {
            get
            {
                var daysFrom = Tran.Timestamp.Date - new DateTime(1972, 5, 28);
                if (daysFrom.Days % 3 == 0) return Brushes.White;
                if (daysFrom.Days % 3 == 1) return new SolidColorBrush(Color.FromRgb(240, 240, 240));
                return new SolidColorBrush(Color.FromRgb(255, 255, 240));
            }
        }

        public Brush TransactionFontColor => Tran.Operation.FontColor();
        #endregion

        #region ' _isSelected '
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }
        #endregion

    }
}