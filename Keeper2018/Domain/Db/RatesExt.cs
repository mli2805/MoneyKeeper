using System;
using System.Linq;

namespace Keeper2018
{
    public static class RatesExt
    {
        public static double AmountInUsd(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            var rateLine = db.Bin.OfficialRates.Last(r => r.Date.Date <= date);
            return currency == CurrencyCode.BYR || currency == CurrencyCode.BYN
                ? (double)amount / rateLine.MyUsdRate.Value
                : currency == CurrencyCode.EUR
                    ? (double)amount * rateLine.NbRates.Euro.Value / rateLine.NbRates.Usd.Value
                    : (double)amount * rateLine.NbRates.Rur.Value / rateLine.NbRates.Rur.Unit / rateLine.NbRates.Usd.Value;

        }
    }
}