using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class RatesExt
    {
        public static string GetRatesMonthDifference(this KeeperDb db, DateTime startDate, DateTime finishMoment)
        {
            var ratesLine = db.Bin.OfficialRates.Last(r => r.Date <= startDate.AddDays(-1) && Math.Abs(r.MyUsdRate.Value) > 0.01);
            double belkaStart = ratesLine.MyUsdRate.Value;
            var ratesLineFinish = db.Bin.OfficialRates.Last(r => r.Date <= finishMoment && Math.Abs(r.MyUsdRate.Value) > 0.01);
            double belkaFinish = ratesLineFinish.MyUsdRate.Value;
            var belkaName = finishMoment < new DateTime(2016, 7, 1) ? "Byr" : "Byn";
            var belkaWord = belkaFinish < belkaStart ? "вырос" : "упал";
            var template = finishMoment < new DateTime(2016, 7, 1) ? "#,0" : "#,0.0000";

            var belka = $"{belkaName} {belkaWord}: {belkaStart.ToString(template)} - {belkaFinish.ToString(template)}";

            var euroStart = ratesLine.NbRates.Euro.Value / belkaStart;
            var euroFinish = ratesLineFinish.NbRates.Euro.Value / belkaFinish;
            var euroWord = euroFinish > euroStart ? "вырос" : "упал";
            var euro = $"Euro {euroWord}: {euroStart:0.000} - {euroFinish:0.000}";

            var rubStart = ratesLine.CbrRate.Usd.Value;
            var rubFinish = ratesLineFinish.CbrRate.Usd.Value;
            var rubWord = rubFinish < rubStart ? "вырос" : "упал";
            var rub = $"Rur {rubWord}: {rubStart:0.0} - {rubFinish:0.0}";

            return $" Изменение курсов     {belka}       {euro}       {rub}";
        }

        public static OneRate GetRate(this KeeperDb db, DateTime dt, CurrencyCode currency)
        {
            var officialRates = db.Bin.OfficialRates.LastOrDefault(r => r.Date <= dt);
            if (officialRates == null) return null;
            switch (currency)
            {
                case CurrencyCode.BYN: return officialRates.NbRates.Usd;
                case CurrencyCode.BYR: return officialRates.NbRates.Usd;
                case CurrencyCode.EUR: return officialRates.NbRates.Euro;
                case CurrencyCode.RUB: return officialRates.NbRates.Rur;
            }
            return null;
        }

        public static decimal AmountInUsd(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            if (currency == CurrencyCode.USD) return amount;
            OfficialRates rateLine;
            if (currency == CurrencyCode.BYR)
                rateLine = date <= new DateTime(2016, 6, 30)
                    ? db.Bin.OfficialRates.Last(r => r.Date.Date <= date)
                    : db.Bin.OfficialRates.First(r => r.Date == new DateTime(2016, 6, 30));
            else if (currency == CurrencyCode.BYN)
                rateLine = db.Bin.OfficialRates.Last(r => r.Date.Date <= date && Math.Abs(r.MyUsdRate.Value) > 0.1);
            else rateLine = db.Bin.OfficialRates.Last(r => r.Date.Date <= date);

            return currency == CurrencyCode.BYR || currency == CurrencyCode.BYN
                ? amount / (decimal)rateLine.MyUsdRate.Value
                : currency == CurrencyCode.EUR
                    ? amount * (decimal)rateLine.NbRates.Euro.Value / (decimal)rateLine.NbRates.Usd.Value
                    : amount * (decimal)rateLine.NbRates.Rur.Value / rateLine.NbRates.Rur.Unit / (decimal)rateLine.NbRates.Usd.Value;
        }

        public static string AmountInUsdString(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount)
        {
            var shortLine = $"{amount} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            var amountInUsd = db.AmountInUsd(date, currency, amount);
            return shortLine + $" ({amountInUsd:#,0.00}$)";
        }
        public static string AmountInUsdString(this KeeperDb db, DateTime date, CurrencyCode? currency, decimal amount, out decimal amountInUsd)
        {
            amountInUsd = amount;
            var shortLine = $"{amount} {currency.ToString().ToLower()}";
            if (currency == CurrencyCode.USD) return shortLine;

            amountInUsd = db.AmountInUsd(date, currency, amount);
            return shortLine + $" ( {amountInUsd:#,0.00} usd )";
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

        public static IEnumerable<string> BalanceReport(this KeeperDb db, DateTime date, Balance balance)
        {
            decimal amountInUsd = 0;
            foreach (var pair in balance.Currencies)
            {
                if (pair.Key == CurrencyCode.USD)
                {
                    amountInUsd = amountInUsd + pair.Value;
                    yield return $"{pair.Value:#,0.00} usd";
                }
                else
                {
                    var inUsd = db.AmountInUsd(date, pair.Key, pair.Value);
                    amountInUsd = amountInUsd + inUsd;
                    yield return $"{pair.Value:#,0.00} {pair.Key.ToString().ToLower()} (= {inUsd:#,0.00} usd)";
                }
            }
            yield return $"Итого {amountInUsd:#,0.00} usd";
        }
    }
}