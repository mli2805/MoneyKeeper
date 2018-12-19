using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class BelBaskets
    {
        private static readonly List<BasketWeights> Baskets = new List<BasketWeights>()
        {
            new BasketWeights() {Usd = 1.0 / 3, Euro = 1.0 / 3, Rur = 1.0 / 3},
            new BasketWeights() {Usd = 0.3, Euro = 0.3, Rur = 0.4},
            new BasketWeights() {Usd = 0.3, Euro = 0.2, Rur = 0.5},
        };

        public static double Calculate(CurrencyRates currencyRates)
        {
            var basket = Math.Pow(currencyRates.NbRates.Usd.Value/currencyRates.NbRates.Usd.Unit, Baskets.Last().Usd) *
                            Math.Pow(currencyRates.NbRates.Euro.Value/currencyRates.NbRates.Euro.Unit, Baskets.Last().Euro) *
                            Math.Pow(currencyRates.NbRates.Rur.Value/currencyRates.NbRates.Rur.Unit, Baskets.Last().Rur);
            return currencyRates.Date < new DateTime(2016,7,1) ? Math.Round(basket, 1) : Math.Round(basket, 4);
        }
    }
}