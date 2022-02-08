﻿using System;
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
            decimal amount, out decimal amountInUsd)
        {
            amountInUsd = amount;
            var shortLine = $"{amount} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            amountInUsd = dataModel.AmountInUsd(date, currency, amount);
            return shortLine + $" ( {amountInUsd:#,0.00} usd )";
        }

        public static decimal AmountInUsd(this KeeperDataModel dataModel, DateTime date, CurrencyCode? currency, decimal amount)
        {
            return dataModel.AmountInUsd(date, currency, amount, out decimal _);
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
            switch (currency)
            {
                case CurrencyCode.BYR:
                    rate = (decimal)rateLine.MyUsdRate.Value;
                    if (date == new DateTime(2016, 7, 1))
                        rate = rate * 10000;
                    return amount / rate;
                case CurrencyCode.BYN:
                    rate = (decimal)rateLine.MyUsdRate.Value;
                    return amount / rate;
                case CurrencyCode.EUR:
                    rate = (decimal)rateLine.NbRates.Euro.Value / (decimal)rateLine.NbRates.Usd.Value;
                    return amount * rate;
                case CurrencyCode.RUB:
                    rate = (decimal)rateLine.NbRates.Rur.Value / rateLine.NbRates.Rur.Unit / (decimal)rateLine.NbRates.Usd.Value;
                    return amount * rate;
                case CurrencyCode.GLD:
                    var line = dataModel.MetalRates.LastOrDefault(m => m.Date <= date);
                    rate = (decimal)((line?.Price ?? 0)  / rateLine.MyUsdRate.Value);
                    return amount * rate;
            }

            return 0;
        }
    }
}