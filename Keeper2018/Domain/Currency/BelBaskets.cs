using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class BelBaskets
    {
        private static readonly List<CurrenciesWeights> Baskets = new List<CurrenciesWeights>()
        {
            new CurrenciesWeights() {Usd = 1.0 / 3, Euro = 1.0 / 3, Rur = 1.0 / 3},
            new CurrenciesWeights() {Usd = 0.3, Euro = 0.3, Rur = 0.4},
            new CurrenciesWeights() {Usd = 0.3, Euro = 0.2, Rur = 0.5},
        };

        public static double Calculate(CurrenciesValues currenciesValues)
        {
            return Math.Pow(currenciesValues.Usd, Baskets.Last().Usd) *
                   Math.Pow(currenciesValues.Euro, Baskets.Last().Euro) *
                   Math.Pow(currenciesValues.Rur, Baskets.Last().Rur);
        }
    }
}