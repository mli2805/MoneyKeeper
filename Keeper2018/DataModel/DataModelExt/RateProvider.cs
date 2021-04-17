using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class RateProvider
    {
        public static OneRate GetRate(this KeeperDataModel dataModel, DateTime dt, CurrencyCode currency, bool isForUsd = false)
        {
            var ratesLine = dataModel.GetRatesLine(dt);
            if (ratesLine == null) return null;
            OneRate result;
            switch (currency)
            {
                case CurrencyCode.BYN: return ratesLine.NbRates.Usd.Clone();
                case CurrencyCode.BYR: return ratesLine.NbRates.Usd.Clone();
                case CurrencyCode.EUR:
                    result = ratesLine.NbRates.Euro.Clone();
                    if (isForUsd)
                        result.Value = result.Value / ratesLine.NbRates.Usd.Value;
                    return result;
                case CurrencyCode.RUB:
                    result = ratesLine.NbRates.Rur.Clone();
                    if (isForUsd)
                        result.Value = ratesLine.NbRates.Usd.Value / (result.Value / result.Unit);
                    return result;

                case CurrencyCode.GLD:
                    var line = dataModel.MetalRates.LastOrDefault(m => m.Date <= dt);
                    result = new OneRate() { Value = line?.Price ?? 0 };
                    if (isForUsd)
                        result.Value = result.Value / ratesLine.MyUsdRate.Value;
                    return result;
            }
            return null;
        }

        public static CurrencyRates GetRatesLine(this KeeperDataModel dataModel, DateTime date)
        {
            CurrencyRates rateLine;
            while (!dataModel.Rates.TryGetValue(date.Date, out rateLine) || (rateLine.MyUsdRate.Value) <= 0.001)
            {
                date = date.AddDays(-1);
            }
            return rateLine;
        }
    }
}