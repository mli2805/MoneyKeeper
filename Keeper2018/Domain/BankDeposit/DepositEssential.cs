using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class DepositEssential
    {
        public DepositCalculationRules CalculationRules { get; set; }
        public List<DepositRateLine> RateLines { get; set; }
    }
}