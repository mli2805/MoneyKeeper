using System;

namespace Keeper.DomainModel
{
    public class DepositTransaction
    {
        public DateTime Timestamp { get; set; }
        public DepositTransactionTypes TransactionType { get; set; }
        public Decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public Decimal AmountInUsd { get; set; }
        public string Comment { get; set; }
    }
}