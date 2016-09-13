using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.DomainModel.Trans
{
    [Serializable]
    public class TranWithTags : PropertyChangedBase
    {
        private DateTime _timestamp;
        private OperationType _operation;
        private Account _myAccount;
        private Account _mySecondAccount;
        private decimal _amount;
        private decimal _amountInReturn;
        private CurrencyCodes? _currency;
        private CurrencyCodes? _currencyInReturn;
        private List<Account> _tags;
        private string _comment;

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange(() => Timestamp);
            }
        }
        public OperationType Operation
        {
            get { return _operation; }
            set
            {
                if (Equals(value, _operation)) return;
                _operation = value;
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
                NotifyOfPropertyChange(() => MyAccount);
            }
        }
        public Account MySecondAccount
        {
            get { return _mySecondAccount; }
            set
            {
                if (Equals(value, _mySecondAccount)) return;
                _mySecondAccount = value;
                NotifyOfPropertyChange(() => MySecondAccount);
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount) return;
                _amount = value;
                NotifyOfPropertyChange(() => Amount);
            }
        }
        public decimal AmountInReturn
        {
            get { return _amountInReturn; }
            set
            {
                if (value == _amountInReturn) return;
                _amountInReturn = value;
                NotifyOfPropertyChange();
            }
        }
        public CurrencyCodes? Currency
        {
            get { return _currency; }
            set
            {
                if (Equals(value, _currency)) return;
                _currency = value;
                NotifyOfPropertyChange(() => Currency);
            }
        }
        public CurrencyCodes? CurrencyInReturn
        {
            get { return _currencyInReturn; }
            set
            {
                if (value == _currencyInReturn) return;
                _currencyInReturn = value;
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
                NotifyOfPropertyChange(() => Tags);
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange(() => Comment);
            }
        }
    }
}
