using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Keeper2018
{
    public static class RatesExt
    {
        private static CurrencyRates GetRatesLine(this KeeperDb db, DateTime date)
        {
            CurrencyRates rateLine;
            while (!db.Bin.Rates.TryGetValue(date.Date, out rateLine) || (rateLine.MyUsdRate.Value) <= 0.001)
            {
                date = date.AddDays(-1);
            }
            return rateLine;
        }

        public static ListOfLines GetRatesMonthDifference(this KeeperDb db, DateTime startDate, DateTime finishMoment)
        {
            var result = new ListOfLines();
            var ratesLine = db.GetRatesLine(startDate);
            double belkaStart = ratesLine.MyUsdRate.Value;

            var ratesLineFinish = db.GetRatesLine(finishMoment);
            double belkaFinish = ratesLineFinish.MyUsdRate.Value;

            var belkaName = finishMoment < new DateTime(2016, 7, 1) ? "Byr" : "Byn";
            var belkaWord = belkaFinish < belkaStart ? "вырос" : "упал";
            var belkaBrush = belkaFinish < belkaStart ? Brushes.Blue : Brushes.Red;
            var template = finishMoment < new DateTime(2016, 7, 1) ? "#,0" : "#,0.0000";

            var belka = $"      {belkaName} {belkaWord}: {belkaStart.ToString(template)} - {belkaFinish.ToString(template)}";
            result.Add(belka, belkaBrush);

            //            var euroStart = ratesLine.NbRates.Euro.Value / ratesLine.NbRates.Usd.Value;
            //            var euroFinish = ratesLineFinish.NbRates.Euro.Value / ratesLineFinish.NbRates.Usd.Value;
            // my rate is more acceptable for this porpose
            var euroStart = ratesLine.MyEurUsdRate.Value;
            var euroFinish = ratesLineFinish.MyEurUsdRate.Value;

            var euroWord = euroFinish > euroStart ? "вырос" : "упал";
            var euroBrush = euroFinish > euroStart ? Brushes.Blue : Brushes.Red;
            var euro = $"      Euro {euroWord}: {euroStart:0.000} - {euroFinish:0.000}";
            result.Add(euro, euroBrush);

            var rubStart = ratesLine.CbrRate.Usd.Value;
            var rubFinish = ratesLineFinish.CbrRate.Usd.Value;
            var rubWord = rubFinish < rubStart ? "вырос" : "упал";
            var rubBrush = rubFinish < rubStart ? Brushes.Blue : Brushes.Red;
            var rub = $"      Rur {rubWord}: {rubStart:0.0} - {rubFinish:0.0}";
            result.Add(rub, rubBrush);

            return result;
        }

        public static OneRate GetRate(this KeeperDb db, DateTime dt, CurrencyCode currency, bool isForUsd = false)
        {
            var ratesLine = db.GetRatesLine(dt);
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
            }
            return null;
        }

        public static decimal AmountInUsd(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            return db.AmountInUsd(date, currency, amount, out decimal _);
        }

        public static decimal AmountInUsd(this KeeperDb db, DateTime date,
            CurrencyCode? currency, decimal amount, out decimal rate)
        {
            rate = 1;
            if (currency == CurrencyCode.USD) return amount;
            var rateLine = db.GetRatesLine(date);
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
            }

            return 0;
        }

        public static decimal AmountInUsd(this KeeperDb db, Transaction tr)
        {
            return db.AmountInUsd(tr.Timestamp, tr.Currency, tr.Amount);
        }


        public static string AmountInUsdString(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            var shortLine = $"{amount} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            var amountInUsd = db.AmountInUsd(date, currency, amount);
            return shortLine + $" ({amountInUsd:#,0.00}$)";
        }

        public static string AmountInUsdWithRate(this KeeperDb db, DateTime date,
            CurrencyCode? currency, decimal amount, out decimal rate)
        {
            rate = 1;
            var shortLine = $"{amount:#,#.00} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            var amountInUsd = db.AmountInUsd(date, currency, amount, out rate);
            return shortLine + $" ({amountInUsd:#,0.00}$)";
        }
        public static string AmountInUsdString(this KeeperDb db, DateTime date, CurrencyCode? currency,
            decimal amount, out decimal amountInUsd)
        {
            amountInUsd = amount;
            var shortLine = $"{amount} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            amountInUsd = db.AmountInUsd(date, currency, amount);
            return shortLine + $" ( {amountInUsd:#,0.00} usd )";
        }

        public static string AmountInUsdString(this KeeperDb db, Transaction tr, out decimal amountIsUsd)
        {
            return db.AmountInUsdString(tr.Timestamp, tr.Currency, tr.Amount, out amountIsUsd);
        }

        public static decimal BalanceInUsd(this KeeperDb db, DateTime date, Balance balance)
        {
            decimal amountInUsd = 0;
            foreach (var pair in balance.Currencies)
            {
                amountInUsd = amountInUsd + (pair.Key == CurrencyCode.USD
                                                  ? pair.Value
                                                  : db.AmountInUsd(date, pair.Key, pair.Value));
            }
            return amountInUsd;
        }

        public static string BalanceInUsdString(this KeeperDb db, DateTime date, Balance balance)
        {
            if (balance.Currencies.All(c => c.Value == 0)) return "0";
            if (balance.Currencies.Count(c => c.Value != 0) > 1) return "more than 1 currency";

            var currency = balance.Currencies.First(c => c.Value != 0).Key;
            var value = balance.Currencies.First(c => c.Value != 0).Value;
            var valueStr = currency == CurrencyCode.BYR ? value.ToString("0,0") : value.ToString("0.00");

            var result = $"{valueStr} {currency.ToString().ToLower()}";
            if (currency != CurrencyCode.USD)
                result = result + $"  ( ${db.AmountInUsd(date, currency, value):#,0.00} )";
            return result;
        }

        public static ListOfLines BalanceReport(this KeeperDb db, DateTime date, Balance balance, bool isRoot)
        {
            var result = new ListOfLines();

            decimal amountInUsd = 0;
            var offset = isRoot ? "" : "   ";
            var fontWeight = isRoot ? FontWeights.Bold : FontWeights.DemiBold;
            foreach (var pair in balance.Currencies)
            {
                if (pair.Key == CurrencyCode.USD)
                {
                    amountInUsd = amountInUsd + pair.Value;
                    result.Add($"{offset}{pair.Value:#,0.00} usd");
                }
                else
                {
                    var inUsd = db.AmountInUsd(date, pair.Key, pair.Value);
                    amountInUsd = amountInUsd + inUsd;
                    result.Add($"{offset}{pair.Value:#,0.00} {pair.Key.ToString().ToLower()} (= {inUsd:#,0.00} usd)");
                }
            }
            result.Add($"{offset}Итого {amountInUsd:#,0.00} usd", fontWeight);

            return result;
        }
    }
}