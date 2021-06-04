using System;
using System.Linq;
using System.Windows;
using KeeperDomain;

namespace Keeper2018
{
    public static class BalanceCalculator
    {
        public static decimal BalanceInUsd(this KeeperDataModel dataModel, DateTime date, Balance balance)
        {
            decimal amountInUsd = 0;
            foreach (var pair in balance.Currencies)
            {
                amountInUsd = amountInUsd + (pair.Key == CurrencyCode.USD
                    ? pair.Value
                    : dataModel.AmountInUsd(date, pair.Key, pair.Value));
            }
            return amountInUsd;
        }

        public static string BalanceInUsdString(this KeeperDataModel dataModel, DateTime date, Balance balance)
        {
            if (balance.Currencies.All(c => c.Value == 0)) return "0";
            if (balance.Currencies.Count(c => c.Value != 0) > 1) return "more than 1 currency";

            var currency = balance.Currencies.First(c => c.Value != 0).Key;
            var value = balance.Currencies.First(c => c.Value != 0).Value;
            var valueStr = currency == CurrencyCode.BYR ? value.ToString("0,0") : value.ToString("0.00");

            var result = $"{valueStr} {currency.ToString().ToLower()}";
            if (currency != CurrencyCode.USD)
                result = result + $"  ( ${dataModel.AmountInUsd(date, currency, value):#,0.00} )";
            return result;
        }

        public static ListOfLines BalanceReport(this KeeperDataModel dataModel, DateTime date, Balance balance, bool isRoot)
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
                    var inUsd = dataModel.AmountInUsd(date, pair.Key, pair.Value);
                    amountInUsd = amountInUsd + inUsd;
                    result.Add($"{offset}{pair.Value:#,0.00} {pair.Key.ToString().ToLower()} (= {inUsd:#,0.00} usd)");
                }
            }
            result.Add($"{offset}Итого {amountInUsd:#,0.00} usd", fontWeight);

            return result;
        }

        public static BalanceWithDetails EvaluateDetails(this Balance balance, KeeperDataModel dataModel, DateTime date)
        {
            var result = new BalanceWithDetails();
            foreach (var pair in balance.Currencies)
            {
                var balanceDetailedLine = new BalanceDetailedLine
                {
                    Currency = pair.Key,
                    Amount = pair.Value,
                    AmountInUsd = pair.Key == CurrencyCode.USD
                        ? pair.Value
                        : dataModel.AmountInUsd(date, pair.Key, pair.Value)
                };
                result.Lines.Add(balanceDetailedLine);
                result.TotalInUsd += balanceDetailedLine.AmountInUsd;
            }

            foreach (var detailedLine in result.Lines)
                detailedLine.PercentOfBalance = detailedLine.AmountInUsd * 100 / result.TotalInUsd;

            return result;
        }
    }
}