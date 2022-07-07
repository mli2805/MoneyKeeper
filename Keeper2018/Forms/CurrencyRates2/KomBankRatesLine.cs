using System;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public class KomBankRatesLine
    {
        public int Id;
        public string Bank;
        public DateTime LastCheck;
        public DateTime StartedFrom;
        public double UsdA;
        public double UsdB;
        public double EurA;
        public double EurB;
        public double RubA;
        public double RubB;
        public double EurUsdA;
        public double EurUsdB;
        public double RubUsdA;
        public double RubUsdB;
        public double RubEurA;
        public double RubEurB;

        public ExchangeRates ToExchangeRates()
        {
            return new ExchangeRates()
            {
                Date = StartedFrom,

                UsdToByn = UsdA,
                BynToUsd = UsdB,
                EurToByn = EurA,
                BynToEur = EurB,
                RubToByn = RubA,
                BynToRub = RubB,
                EurToUsd = EurUsdA,
                UsdToEur = EurUsdB,
                UsdToRub = RubUsdA,
                RubToUsd = RubUsdB,
                EurToRub = RubEurA,
                RubToEur = RubEurB,
            };
        }
    }
}
