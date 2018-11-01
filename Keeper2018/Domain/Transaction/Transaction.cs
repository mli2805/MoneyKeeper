using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class Transaction
    {
        [Serializable]
        public class TranWithTags 
        {
            public DateTime Timestamp;
            public OperationType Operation;
            public Account MyAccount;
            public Account MySecondAccount;
            public decimal Amount;
            public decimal AmountInReturn;
            public CurrencyCode? Currency;
            public CurrencyCode? CurrencyInReturn;
            public List<Account> Tags;
            public string Comment;
        }
    }
}
