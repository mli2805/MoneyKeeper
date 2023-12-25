using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AmountInUsdProvider
    {
        public static string AmountInUsdString(this KeeperDataModel dataModel, DateTime date, CurrencyCode? currency, decimal amount)
        {
            return dataModel.AmountInUsdString(date, currency, amount, out decimal _);
        }

        public static string AmountInUsdString(this KeeperDataModel dataModel, DateTime date, CurrencyCode? currency,
            decimal amount, out decimal amountInUsd, bool flag = true)
        {
            amountInUsd = amount;
            var shortLine = $"{amount:N} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            amountInUsd = dataModel.AmountInUsd(date, currency, amount);
            return shortLine + (flag ? $" ( {amountInUsd:#,0.00} usd )" :  $" ({amountInUsd:#,0.00} usd)");
        }

        public static decimal AmountInUsd(this KeeperDataModel dataModel, DateTime date, CurrencyCode? currency, decimal amount)
        {
            return dataModel.AmountInUsd(date, currency, amount, out decimal _);
        }

        public static decimal GetAmountInUsd(this TransactionModel transactionModel, KeeperDataModel dataModel)
        {
            return dataModel.AmountInUsd(transactionModel.Timestamp, transactionModel.Currency, transactionModel.Amount,
                out decimal _);
        }

        public static string AmountInUsdWithRate(this KeeperDataModel dataModel, DateTime date,
            CurrencyCode? currency, decimal amount, out decimal rate)
        {
            rate = 1;
            var shortLine = $"{amount:#,#.00} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            var amountInUsd = dataModel.AmountInUsd(date, currency, amount, out rate);
            return shortLine + $" ({amountInUsd:#,0.00}$)";
        }

        private static decimal AmountInUsd(this KeeperDataModel dataModel, DateTime date,
            CurrencyCode? currency, decimal amount, out decimal rate)
        {
            rate = 1;
            if (currency == CurrencyCode.USD) return amount;
            var rateLine = dataModel.GetRatesLine(date);
            var exchangeRatesLine = dataModel.GetExchangeRatesLine(date);
            switch (currency)
            {
                case CurrencyCode.BYR:
                    rate = (decimal)exchangeRatesLine.BynToUsd;
                    if (date == new DateTime(2016, 7, 1))
                        rate = rate * 10000;
                    return amount / rate;
                case CurrencyCode.BYN:
                    rate = (decimal)exchangeRatesLine.BynToUsd;
                    return amount / rate;
                case CurrencyCode.EUR:
                    rate = (decimal)rateLine.NbRates.Euro.Value / (decimal)rateLine.NbRates.Usd.Value;
                    return amount * rate;
                case CurrencyCode.RUB:
                    rate = (decimal)exchangeRatesLine.RubToUsd;
                    return rate == 0 
                        ? amount * (decimal)rateLine.NbRates.Rur.Value / rateLine.NbRates.Rur.Unit / (decimal)rateLine.NbRates.Usd.Value
                        : amount / rate;
                case CurrencyCode.GLD:
                    var line = dataModel.MetalRates.LastOrDefault(m => m.Date <= date);
                    rate = (decimal)((line?.Price ?? 0)  / exchangeRatesLine.BynToUsd);
                    return amount * rate;
            }

            return 0;
        }
    }
}