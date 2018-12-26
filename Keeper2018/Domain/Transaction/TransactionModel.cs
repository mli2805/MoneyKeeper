using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactionModel : PropertyChangedBase
    {
        public DateTime Timestamp { get; set; }
        public int OrdinalInDate { get; set; }
        public int Receipt { get; set; }
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