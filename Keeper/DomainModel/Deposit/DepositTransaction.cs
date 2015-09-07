using System;

namespace Keeper.DomainModel.Deposit
{
    public class DepositTransaction
    {
        public DateTime Timestamp { get; set; }
        public DepositTransactionTypes TransactionType { get; set; }
        public Decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public Decimal AmountInUsd { get; set; }
        public string Comment { get; set; }

        public int Destination()
        {
            return TransactionType == DepositTransactionTypes.Расход || TransactionType == DepositTransactionTypes.ОбменРасход ? -1 : 1;
        }
    }
}