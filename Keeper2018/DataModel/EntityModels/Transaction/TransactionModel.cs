using System;
using System.Collections.Generic;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TransactionModel : PropertyChangedBase
    {
        public int Id;

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange();
            }
        }

        public int Receipt { get; set; }
        public OperationType Operation { get; set; }
        public PaymentWay PaymentWay { get; set; }

        private AccountModel _myAccount;
        public AccountModel MyAccount
        {
            get => _myAccount;
            set
            {
                if (Equals(value, _myAccount)) return;
                _myAccount = value;
                NotifyOfPropertyChange();
            }
        }

        private AccountModel _mySecondAccount;
        public AccountModel MySecondAccount
        {
            get => _mySecondAccount;
            set
            {
                if (Equals(value, _mySecondAccount)) return;
                _mySecondAccount = value;
                NotifyOfPropertyChange();
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (value == _amount) return;
                _amount = value;
                NotifyOfPropertyChange();
            }
        }

        private decimal _amountInReturn;

        public decimal AmountInReturn
        {
            get => _amountInReturn;
            set
            {
                if (value == _amountInReturn) return;
                _amountInReturn = value;
                NotifyOfPropertyChange();
            }
        }

        private CurrencyCode _currency;
        public CurrencyCode Currency
        {
            get => _currency;
            set
            {
                if (value == _currency) return;
                _currency = value;
                NotifyOfPropertyChange();
            }
        }

        private CurrencyCode? _currencyInReturn;
        public CurrencyCode? CurrencyInReturn
        {
            get => _currencyInReturn;
            set
            {
                if (value == _currencyInReturn) return;
                _currencyInReturn = value;
                NotifyOfPropertyChange();
            }
        }

        public List<AccountModel> Tags { get; set; }
        public string Comment { get; set; }
    }
}