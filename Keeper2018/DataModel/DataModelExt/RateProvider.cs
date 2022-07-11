using System;
using System.Linq;
using KeeperDomain;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public static class RateProvider
    {
        public static double GetRubBynRate(this KeeperDataModel dataModel, DateTime dt)
        {
            var ratesLine = dataModel.GetRatesLine(dt);
            if (ratesLine == null) return 0;
            return ratesLine.NbRates.Rur.Unit / ratesLine.NbRates.Rur.Value;
        }

        public static OneRate GetRate(this KeeperDataModel dataModel, DateTime dt, CurrencyCode currency, bool isForUsd = false)
        {
            var ratesLine = dataModel.GetRatesLine(dt);
            if (ratesLine == null) return null;
            var exchangeRatesLine = dataModel.GetExchangeRatesLine(dt);
            OneRate result;
            switch (currency)
            {
                case CurrencyCode.BYN: 
                case CurrencyCode.BYR: 
                    return new OneRate() { Value = exchangeRatesLine.BynToUsd, Unit = 1 };
                case CurrencyCode.EUR:
                    if (isForUsd)
                    {
                        result = new OneRate() { Value = exchangeRatesLine.EurToUsd, Unit = 1 };
                        if (result.Value == 0)
                            result.Value = ratesLine.NbRates.EuroUsdCross;
                        return result;
                    }
                    else
                    {
                        result = new OneRate() { Value = exchangeRatesLine.EurToByn, Unit = 1 };
                        if (result.Value == 0)
                            result.Value = ratesLine.NbRates.EuroUsdCross * exchangeRatesLine.BynToUsd;
                        return result;
                    }
                case CurrencyCode.RUB:
                    if (isForUsd)
                    {
                        result = new OneRate() { Value = exchangeRatesLine.RubToUsd, Unit = 1 };
                        if (result.Value == 0)
                            result = ratesLine.CbrRate.Usd;
                        return result;
                    }
                    else
                    {
                        result = new OneRate() { Value = exchangeRatesLine.RubToByn, Unit = 100 };
                        if (result.Value == 0)
                            result.Value = ratesLine.NbRates.Rur.Value ;
                        return result;
                    }
                case CurrencyCode.GLD:
                    var line = dataModel.MetalRates.LastOrDefault(m => m.Date <= dt);
                    result = new OneRate() { Value = line?.Price ?? 0 };
                    if (isForUsd)
                        result.Value /= exchangeRatesLine.BynToUsd;
                    return result;
            }
            return null;
        }

        public static OfficialRates GetRatesLine(this KeeperDataModel dataModel, DateTime date)
        {
            OfficialRates rateLine;
            while (!dataModel.OfficialRates.TryGetValue(date.Date, out rateLine))
            {
                date = date.AddDays(-1);
            }
            return rateLine;
        }

        public static ExchangeRates GetExchangeRatesLine(this KeeperDataModel keeperDataModel, DateTime date)
        {
            while (!keeperDataModel.ExchangeRates.ContainsKey(date.Date))
            {
                date = date.AddDays(-1);
            }

            return keeperDataModel.ExchangeRates[date.Date];
        }
    }
}