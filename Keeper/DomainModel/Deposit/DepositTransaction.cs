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

        public string Counteragent { get; set; }
        public string Comment { get; set; }

        public bool IsIncome()
        {
            return !(TransactionType == DepositTransactionTypes.������ || TransactionType == DepositTransactionTypes.�����������);
        }
        public int Destination()
        {
            return IsIncome() ? 1 : -1;
        }
    }
}