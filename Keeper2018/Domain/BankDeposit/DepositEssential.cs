using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
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
}