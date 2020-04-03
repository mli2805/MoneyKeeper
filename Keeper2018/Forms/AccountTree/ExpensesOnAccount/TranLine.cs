using System;
using KeeperDomain;

namespace Keeper2018.ExpensesOnAccount
{
    public class TranLine
    {
        public DateTime Timestamp { get; set; }
        public string Category { get; set; }
        public string CounterpartyName { get; set; }
        public decimal Amount { get; set; }
        public string AmountStr => Amount.ToString("#,0.##") + " " + Currency.ToString().ToLower();
        public CurrencyCode Currency { get; set; }
        public string Comment { get; set; }
    }
}