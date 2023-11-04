using System;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class BankAccountModel : PropertyChangedBase
    {
        public int Id { get; set; } // совпадает с ID Account'a

        public int BankId { get; set; }
        public int DepositOfferId { get; set; } // не только по депозиту, но и по карте и даже расчетному счету могут начисляться %%

        private CurrencyCode _mainCurrency;

        public CurrencyCode MainCurrency
        {
            get => _mainCurrency;
            set
            {
                if (value == _mainCurrency) return;
                _mainCurrency = value;
                NotifyOfPropertyChange();
            }
        }

        public string AgreementNumber { get; set; } // номер договора, где он есть
        public string ReplenishDetails { get; set; } // реквизиты для пополнения


        public DateTime StartDate { get; set; }

        private DateTime _finishDate;
        public DateTime FinishDate
        {
            get => _finishDate;
            set
            {
                if (value.Equals(_finishDate)) return;
                _finishDate = value;
                NotifyOfPropertyChange();
            }
        } // max для расчётных счетов

        public Visibility FinishVisibility => Deposit != null || PayCard != null ? Visibility.Visible : Visibility.Hidden;

        public bool IsMine { get; set; } 

        public Deposit Deposit { get; set; }
        public PayCard PayCard { get; set; }
      

    }
}