using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public static class TranToAssetAnalysis
    {
        public static IEnumerable<string> ToAssetAnalysis(this InvestTranModel tranModel)
        {
            string word = tranModel.WordsForOperationType();

            decimal sum;
            decimal oneAssetPrice;
            string oneAssetPriceStr;
            string oneAssetPriceStr2;
            if (tranModel.InvestOperationType.IsBond())
            {
                oneAssetPrice = tranModel.CurrencyAmount / tranModel.AssetAmount +
                                tranModel.CouponAmount / tranModel.AssetAmount;
                sum = tranModel.CurrencyAmount + tranModel.CouponAmount;
                oneAssetPriceStr = $"{tranModel.CurrencyAmount / tranModel.AssetAmount:N} + {tranModel.CouponAmount / tranModel.AssetAmount:N}";
                oneAssetPriceStr2 = $" = {oneAssetPrice} {tranModel.Currency.ToString().ToLower()}";
            }
            else
            {
                oneAssetPrice = tranModel.CurrencyAmount / tranModel.AssetAmount;
                sum = tranModel.CurrencyAmount;
                oneAssetPriceStr = $"{tranModel.CurrencyAmount / tranModel.AssetAmount:N}";
                oneAssetPriceStr2 = "";
            }

            var inCurrency = $"{oneAssetPrice:N} * {tranModel.AssetAmount} шт = {sum:N} {tranModel.Currency.ToString().ToLower()}";

            if (tranModel.Asset.TrustAccount.Currency == CurrencyCode.USD)
            {
                yield return $"{tranModel.Timestamp:dd-MMM-yy} {word}";
                yield return $"    {inCurrency}";
            }
            else
            {
                string rate = $"(курс: {tranModel.Rate:F} rub купить 1 usd)";
                var inUsd = $" ({sum / tranModel.Rate:N} usd)";
                var oneAssetPriceStrInUsd = $"({oneAssetPrice / tranModel.Rate:N} usd)";
                yield return $"{tranModel.Timestamp:dd-MMM-yy} {rate}";
                yield return $" {word} {oneAssetPriceStr}{oneAssetPriceStr2} {oneAssetPriceStrInUsd}";
                yield return $"         {inCurrency} {inUsd}";
            }
            yield return "";
        }

        private static string WordsForOperationType(this InvestTranModel tranModel)
        {
            switch (tranModel.InvestOperationType)
            {
                case InvestOperationType.BuyStocks:
                case InvestOperationType.BuyBonds:
                    return "купил по";
                case InvestOperationType.SellStocks:
                case InvestOperationType.SellBonds:
                    return "продал по";
                case InvestOperationType.EnrollCouponOrDividends:
                    return "получены дивы/купон";
                default:
                    return "not implemented yet";
            }
        }
    }
}