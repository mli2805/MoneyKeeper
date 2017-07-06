using System;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Deposit
{
    public class DepositTransaction
    {
        public DateTime Timestamp { get; set; }
        public DepositTransactionTypes TransactionType { get; set; }
        public Decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public Decimal AmountInUsd { get; set; }

        public string Counteragent { get; set; }
        public string Comment { get; set; }

        public bool IsIncome()
        {
            return !(TransactionType == DepositTransactionTypes.Расход || TransactionType == DepositTransactionTypes.ОбменРасход);
        }
        public int Destination()
        {
            return IsIncome() ? 1 : -1;
        }

        public string AmountToString()
        {
            return Currency == CurrencyCodes.BYR ? $"{Amount:#,0.00}" : $"{Amount:#,0}";
        }
    }
}