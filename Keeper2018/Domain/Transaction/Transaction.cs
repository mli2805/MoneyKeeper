using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class Transaction
    {
        public DateTime Timestamp;
        public int OrdinalInDate;
        public OperationType Operation;
        public int MyAccount;
        public int MySecondAccount;
        public decimal Amount;
        public decimal AmountInReturn;
        public CurrencyCode Currency;
        public CurrencyCode? CurrencyInReturn;
        public List<int> Tags;
        public string Comment;
    }

    public class TransactionModel
    {
        public DateTime Timestamp { get; set; }
        public int OrdinalInDate { get; set; }
        public OperationType Operation { get; set; }
        public AccountModel MyAccount { get; set; }
        public AccountModel MySecondAccount { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountInReturn { get; set; }
        public CurrencyCode Currency { get; set; }
        public CurrencyCode? CurrencyInReturn { get; set; }
        public List<AccountModel> Tags { get; set; }
        public string Comment { get; set; }
    }
}
