using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018.Basket
{
    public static class BelBaskets
    {
        private static readonly List<BasketWeights> Baskets = new List<BasketWeights>()
        {
            new BasketWeights() {Usd = 1.0 / 3, Euro = 1.0 / 3, Rur = 1.0 / 3},
            new BasketWeights() {Usd = 0.3, Euro = 0.3, Rur = 0.4},
            new BasketWeights() {Usd = 0.3, Euro = 0.2, Rur = 0.5},
        };

        public static double Calculate(OfficialRates officialRates)
        {
            var basket = Math.Pow(officialRates.NbRates.Usd.Value/officialRates.NbRates.Usd.Unit, Baskets.Last().Usd) *
                            Math.Pow(officialRates.NbRates.Euro.Value/officialRates.NbRates.Euro.Unit, Baskets.Last().Euro) *
                            Math.Pow(officialRates.NbRates.Rur.Value/officialRates.NbRates.Rur.Unit, Baskets.Last().Rur);
            return officialRates.Date < new DateTime(2016,7,1) ? Math.Round(basket, 1) : Math.Round(basket, 4);
        }
    }
}