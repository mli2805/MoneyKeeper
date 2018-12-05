using System;
using System.Collections.Generic;
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
        public Dictionary<DateTime, DepositEssential> Essentials {get; set; }
        public string Comment { get; set; }
    }

    [Serializable]
    public class DepositEssential
    {
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
    }

    [Serializable]
    public class DepositRateLine
    {
        public int DepositOfferId;
        public DateTime DateFrom { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public decimal Rate { get; set; }

        public DepositRateLine ShallowCopy()
        {
            return (DepositRateLine) MemberwiseClone();
        }
    }
}