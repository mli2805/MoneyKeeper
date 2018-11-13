using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class Transaction
    {
        public DateTime Timestamp;
        public OperationType Operation;
        public int MyAccount;
        public int MySecondAccount;
        public double Amount;
        public double AmountInReturn;
        public CurrencyCode? Currency;
        public CurrencyCode? CurrencyInReturn;
        public List<int> Tags;
        public string Comment;
    }

    public class TransactionModel
    {
        public DateTime Timestamp;
        public OperationType Operation;
        public AccountModel MyAccount;
        public AccountModel MySecondAccount;
        public double Amount;
        public double AmountInReturn;
        public CurrencyCode? Currency;
        public CurrencyCode? CurrencyInReturn;
        public List<AccountModel> Tags;
        public string Comment;
    }
}
