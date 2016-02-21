﻿using System;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper.DomainModel.Transactions
{
    public class TranCocoon : PropertyChangedBase
    {
        public TranWithTags Tran { get; set; }

        public string AccountForDatagrid => GetAccountForDatagrid();
        public string AmountForDatagrid => GetAmountForDatagrid();
        public string TagsForDatagrid => GetTagsForDatagrid();

        private string GetAccountForDatagrid()
        {
            return IsOneAccountTransaction() ? Tran.MyAccount.Name : $"{Tran.MyAccount.Name} ->\n  {Tran.MySecondAccount.Name}";
        }
        private string GetAmountForDatagrid()
        {
            return IsOneAmountTransaction() 
                ? ShowAmount(Tran.Amount, Tran.Currency) 
                : ShowAmount(Tran.Amount, Tran.Currency) + " ->\n    " + ShowAmount(Tran.AmountInReturn, Tran.CurrencyInReturn);
        }
        private string ShowAmount(decimal amount, CurrencyCodes currency)
        {
            return currency == CurrencyCodes.BYR
                ? $" {amount:#,0} {currency.ToString().ToLower()}"
                : $" {amount:#,0.00} {currency.ToString().ToLower()}";
        }
        private string GetTagsForDatagrid()
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
                     Tran.Operation == OperationType.Обмен;
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
                if (daysFrom.Days % 4 == 0) return Brushes.Cornsilk;
                if (daysFrom.Days % 4 == 1) return new SolidColorBrush(Color.FromRgb(240, 255, 240));
                if (daysFrom.Days % 4 == 2) return Brushes.GhostWhite;
                return Brushes.Azure;
            }
        }
        public Brush TransactionFontColor
        {
            get
            {
                if (Tran.Operation == OperationType.Доход) return Brushes.Blue;
                if (Tran.Operation == OperationType.Расход) return Brushes.Red;
                if (Tran.Operation == OperationType.Перенос) return Brushes.Black;
                if (Tran.Operation == OperationType.Обмен || Tran.Operation == OperationType.ОбменПеренос) return Brushes.DarkGreen;
                if (Tran.Operation == OperationType.Форекс) return Brushes.LightSeaGreen;
                return Brushes.Gray;
            }
        }
        #endregion

        #region ' _isSelected '
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
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
