using System;

namespace Keeper.DomainModel.Deposit
{
    [Serializable]
    public class DepositRateLine
    {
        public DateTime DateFrom { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public decimal Rate { get; set; }

        [NonSerialized] public int AccountId;

    }
}