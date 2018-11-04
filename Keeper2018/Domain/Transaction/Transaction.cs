using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class Transaction
    {
        public DateTime Timestamp;
        public OperationType Operation;
        public Account MyAccount;
        public Account MySecondAccount;
        public double Amount;
        public double AmountInReturn;
        public CurrencyCode? Currency;
        public CurrencyCode? CurrencyInReturn;
        public List<Account> Tags;
        public string Comment;
    }
}
