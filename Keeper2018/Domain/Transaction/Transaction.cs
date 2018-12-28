using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class Transaction
    {
        public DateTime Timestamp;
       // public int OrdinalInDate;
        public int Receipt;
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
}
