using System;

namespace Keeper2018
{
    [Serializable]
    public class DepositRateLine
    {
        public int DepositOfferId;
        public DateTime DateFrom { get; set; }
        public double AmountFrom { get; set; }
        public double AmountTo { get; set; }
        public double Rate { get; set; }

        public DepositRateLine ShallowCopy()
        {
            return (DepositRateLine) MemberwiseClone();
        }
    }
}