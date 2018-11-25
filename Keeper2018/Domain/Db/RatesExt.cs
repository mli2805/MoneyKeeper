using System;
using System.Linq;

namespace Keeper2018
{
    public static class RatesExt
    {
        public static decimal AmountInUsd(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            var rateLine = db.Bin.OfficialRates.Last(r => r.Date.Date <= date);
            return currency == CurrencyCode.BYR || currency == CurrencyCode.BYN
                ? amount / (decimal)rateLine.MyUsdRate.Value
                : currency == CurrencyCode.EUR
                    ? amount * (decimal)rateLine.NbRates.Euro.Value / (decimal)rateLine.NbRates.Usd.Value
                    : amount * (decimal)rateLine.NbRates.Rur.Value / rateLine.NbRates.Rur.Unit / (decimal)rateLine.NbRates.Usd.Value;

        }

        public static string AmountInUsdString(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            var shortLine = $"{amount} {currency.ToString().ToLower()}";
            if (currency != CurrencyCode.USD)
            {
                var amountInUsd = db.AmountInUsd(date, currency, amount);
                shortLine = shortLine + $" ({amountInUsd:#.00}$)";
            }
            return shortLine;
        }
    }
}