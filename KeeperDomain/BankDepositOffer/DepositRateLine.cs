using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class DepositRateLine
    {
        public int Id { get; set; } //PK

        public int DepositOfferId;

        public int DepositOfferEssentialsId;

        public DateTime DateFrom { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public decimal Rate { get; set; }

        public DepositRateLine ShallowCopy()
        {
            return (DepositRateLine) MemberwiseClone();
        }

        public string PartDump()
        {
            return DepositOfferId + " ; " + DepositOfferEssentialsId  + " ; " + 
                   $"{DateFrom:dd/MM/yyyy}" + " ; " + AmountFrom.ToString(new CultureInfo("en-US")) + " ; "
                   + AmountTo.ToString(new CultureInfo("en-US")) + " ; " + Rate.ToString(new CultureInfo("en-US"));
        }
    }
}