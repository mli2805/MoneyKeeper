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

        public static double Calculate(NbRbRate nbRbRate)
        {
            var basket = Math.Pow(nbRbRate.Usd.Value/nbRbRate.Usd.Unit, Baskets.Last().Usd) *
                            Math.Pow(nbRbRate.Euro.Value/nbRbRate.Euro.Unit, Baskets.Last().Euro) *
                            Math.Pow(nbRbRate.Rur.Value/nbRbRate.Rur.Unit, Baskets.Last().Rur);
            return nbRbRate.Date < new DateTime(2016,7,1) ? Math.Round(basket, 1) : Math.Round(basket, 4);
        }
    }
}