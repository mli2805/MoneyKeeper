using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Keeper2018
{
    [Serializable]
    public class DepositOffer
    {
        public int Id { get; set; }
        public int Bank { get; set; }
        public string Title { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        // Conditions of offer could be changed (especially rates, initial sum while Title remains the same)
        // only for newly opened deposits
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepositEssential> Essentials {get; set; } = new Dictionary<DateTime, DepositEssential>();
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + Bank + " ; " + Title + " ; " + MainCurrency + " ; " + Comment;
        }
    }

    [Serializable]
    public class DepositEssential
    {
        // ID is necessary because Deposit can have more than one Essential 
        // and txt file should have ID to separate RateLines from different Essentials
        public int Id { get; set; } 
        public DepositCalculationRules CalculationRules { get; set; } = new DepositCalculationRules();
        public List<DepositRateLine> RateLines { get; set; } = new List<DepositRateLine>();

        public DepositEssential DeepCopy()
        {
            return new DepositEssential
            {
                CalculationRules = CalculationRules.ShallowCopy(),
                RateLines = new List<DepositRateLine>(RateLines.Select(l => l.ShallowCopy()))
            };
        }
        public string Comment { get; set; }

        public string PartDump()
        {
            return Id + " ; " + CalculationRules.Dump() + " ; " + Comment;
        }
    }

    [Serializable]
    public class DepositRateLine
    {
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