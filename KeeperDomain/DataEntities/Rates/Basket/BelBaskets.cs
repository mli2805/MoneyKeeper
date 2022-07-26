using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperDomain.Basket
{
    public static class BelBaskets
    {
        private static readonly List<BasketWeights> Baskets = new List<BasketWeights>()
        {
            new BasketWeights() {DateFrom = new DateTime(2009, 1, 1), Usd = 1.0 / 3, Euro = 1.0 / 3, Rur = 1.0 / 3},
            new BasketWeights() {DateFrom = new DateTime(2016, 1, 1), Usd = 0.3, Euro = 0.3, Rur = 0.4}, // дата примерно
            new BasketWeights() {DateFrom = new DateTime(2016, 11, 1), Usd = 0.3, Euro = 0.2, Rur = 0.5},
            new BasketWeights() {DateFrom = new DateTime(2022, 7, 15), Usd = 0.3, Euro = 0.1, Rur = 0.5, Cny = 0.1},
        };

        public static double CalculateOld(OfficialRates officialRates)
        {
            var basket = Math.Pow(officialRates.NbRates.Usd.Value / officialRates.NbRates.Usd.Unit, Baskets.Last().Usd) *
                            Math.Pow(officialRates.NbRates.Euro.Value / officialRates.NbRates.Euro.Unit, Baskets.Last().Euro) *
                            Math.Pow(officialRates.NbRates.Rur.Value / officialRates.NbRates.Rur.Unit, Baskets.Last().Rur);
            return officialRates.Date < new DateTime(2016, 7, 1) ? Math.Round(basket, 1) : Math.Round(basket, 4);
        }

        public static double Calculate(OfficialRates officialRates)
        {
            var weights = Baskets.First(b => b.DateFrom == new DateTime(2022, 7, 15));

            var basket = Math.Pow(officialRates.NbRates.Usd.Value / officialRates.NbRates.Usd.Unit, weights.Usd) *
                         Math.Pow(officialRates.NbRates.Euro.Value / officialRates.NbRates.Euro.Unit, weights.Euro) *
                         Math.Pow(officialRates.NbRates.Rur.Value / officialRates.NbRates.Rur.Unit, weights.Rur) *
                         Math.Pow(officialRates.NbRates.Cny.Value / officialRates.NbRates.Cny.Unit, weights.Cny);
            return officialRates.Date < new DateTime(2016, 7, 1) ? Math.Round(basket, 1) : Math.Round(basket, 4);
        }
    }
}