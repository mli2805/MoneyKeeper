﻿using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AssetOnPeriodReportComposer
    {
        public static AssetOnPeriodReportModel CreateReport(this AssetOnPeriodData assetOnPeriodData)
        {
            return new AssetOnPeriodReportModel()
            {
                BeforeState = assetOnPeriodData.Before.CreateBeforeColumn().ToList(),
                OnStartState = assetOnPeriodData.CreateOnStartColumn().ToList(),
                InBetweenTrans = assetOnPeriodData.CreateInBetweenColumn().ToList(),
                AtEndState = assetOnPeriodData.CreateAtEndColumn().ToList(),
            };
        }

        private static List<string> CreateBeforeColumn(this AssetState assetState)
        {
            List<string> result = new List<string>();
            result.Add(assetState.Caption);
            result.Add("");
            if (!assetState.Trans.Any()) return result;

            foreach (var tran in assetState.Trans
                         .Where(t => t.InvestOperationType.IsBuySell()))
                result.AddRange(tran.ToAssetAnalysis());

            result.Add($"Количество: {assetState.Quantity} шт.");
            result.Add($" средняя цена: {assetState.AveragePriceStr}");
            result.Add($" цена пакета: {assetState.PriceStr}");
            result.Add("");

            result.Add("С учетом оплаченных");
            result.Add($" комиссий {assetState.OperationFeesStr}");
            result.Add("получились:");
            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            result.Add($" средняя цена: {averagePrice:N} usd");
            result.Add($" и цена пакета: {fullPrice:N} usd");
            return result;
        }

        private static IEnumerable<string> CreateOnStartColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var assetStateBefore = assetOnPeriodData.Before;
            var assetState = assetOnPeriodData.OnStart;
            result.Add(assetState.Caption);
            if (assetState.Asset.TrustAccount.Currency == CurrencyCode.RUB)
                result.Add(assetState.CurrentCurrencyRateStr);
            result.Add("");
            if (assetState.Price == 0) return result;

            result.Add($"Количество: {assetState.Quantity} шт.");
            result.Add(assetStateBefore.Asset.AssetType == AssetType.Bond
                ? $" цена + НКД: ({assetState.CurrentAssetRate:N} + {assetState.AccumulatedCouponIncome:F}) = {assetState.AveragePriceStr}"
                : $" цена: {assetState.AveragePriceStr}");
            result.Add($"Стоимость: {assetState.PriceStr}");
            result.Add("");

            var fullPrice = assetStateBefore.PriceInUsd + assetState.OperationFeesInUsd - assetState.ReceivedCouponInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            if (assetState.ReceivedCouponInUsd > 0)
            {
                result.Add($"Получен купон (дивы): {assetState.ReceivedCouponStr}");
                result.Add("  можно сказать уменьшены:");
                result.Add($" средняя цена: {averagePrice:N} usd");
                result.Add($" и стоимость покупки: {fullPrice:N} usd");
                result.Add("");
            }

            result.Add(assetState.Caption);
            var profitInUsd = assetState.PriceInUsd - fullPrice;
            var word = profitInUsd > 0 ? "прибыль составляла: " : "убыток составлял";
            result.Add($"{word} {profitInUsd:N} usd");
            return result;
        }

        private static IEnumerable<string> CreateInBetweenColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var assetState = assetOnPeriodData.InBetween;
            result.Add(assetState.Caption);
            if (assetState.Asset.TrustAccount.Currency == CurrencyCode.RUB)
            {
                var currencyLine = GetChangesLineForTwoFigures(
                    assetOnPeriodData.OnStart.CurrentCurrencyRate, assetState.CurrentCurrencyRate, false);
                result.Add($"   курс рубля {currencyLine} ");
            }
            result.Add("");

            foreach (var tran in assetState.Trans
                         .Where(t => t.InvestOperationType.IsBuySell() || t.InvestOperationType == InvestOperationType.EnrollCouponOrDividends))
                result.AddRange(tran.ToAssetAnalysis());
            result.Add("");

            if (assetState.OperationFees != 0)
                result.Add($"Оплачены комиссии {assetState.OperationFeesStr}");
            //if (assetState.ReceivedCoupon != 0)
            //    result.Add($"Получен купон (дивы): {assetState.ReceivedCouponStr}");
            return result;
        }

        private static IEnumerable<string> CreateAtEndColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var currentState = assetOnPeriodData.AtEnd;
            result.Add(currentState.Caption);
            if (currentState.Asset.TrustAccount.Currency == CurrencyCode.RUB)
                result.Add(currentState.CurrentCurrencyRateStr);
            result.Add("");

            result.AddRange(currentState.FillState());
            result.AddRange(currentState.Quantity > 0
                ? CreateAtEndColumnForExistingAsset(assetOnPeriodData)
                : CreateAtEndColumnForAssetSoldBefore(assetOnPeriodData));


            return result;
        }

        private static List<string> CreateAtEndColumnForAssetSoldBefore(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            if (assetOnPeriodData.OnStart.Price > 0)
                result.AddRange(assetOnPeriodData.FillChangesAndProfit());
            return result;
        }

        private static List<string> CreateAtEndColumnForExistingAsset(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();

            if (assetOnPeriodData.OnStart.Price > 0)
                result.AddRange(assetOnPeriodData.FillChangesAndProfit());
            return result;
        }

        private static List<string> FillState(this AssetState currentState)
        {
            List<string> result = new List<string>();
            result.Add($"Количество: {currentState.Quantity} шт.");

            if (currentState.Quantity > 0)
            {
                result.Add(currentState.Asset.AssetType == AssetType.Bond
                    ? $" цена + НКД: ({currentState.CurrentAssetRate:N} + {currentState.AccumulatedCouponIncome:F}) = {currentState.AveragePriceStr}"
                    : $" цена: {currentState.AveragePriceStr}");
                result.Add($"Стоимость: {currentState.PriceStr}");
            }
            result.Add("");
            return result;
        }

        private static List<string> FillChangesAndProfit(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var currentState = assetOnPeriodData.AtEnd;
            result.Add($"За период {assetOnPeriodData.InBetween.Caption}");
            result.AddRange(FillPeriodChanges(assetOnPeriodData.OnStart, currentState));
            var profitOverPeriod = currentState.PriceInUsd - assetOnPeriodData.OnStart.PriceInUsd
                                                           - assetOnPeriodData.InBetween.OperationFeesInUsd + assetOnPeriodData.InBetween.ReceivedCouponInUsd;
            var resultStr = $"Результат в usd: {currentState.PriceInUsd:F} - {assetOnPeriodData.OnStart.PriceInUsd:F}";
            if (assetOnPeriodData.InBetween.OperationFeesInUsd > 0)
                resultStr += $" - {assetOnPeriodData.InBetween.OperationFeesInUsd:F}";
            if (assetOnPeriodData.InBetween.ReceivedCouponInUsd > 0)
                resultStr += $" + {assetOnPeriodData.InBetween.ReceivedCouponInUsd:F}";
            result.Add(resultStr);
            result.Add($"                        = {profitOverPeriod:N} usd  ({profitOverPeriod / assetOnPeriodData.OnStart.PriceInUsd * 100:F1}%)");
            result.Add("");

            result.Add("С момента приобретения");
            result.AddRange(FillAllChanges(assetOnPeriodData));
            var fullPriceBefore = assetOnPeriodData.Before.PriceInUsd + assetOnPeriodData.Before.OperationFeesInUsd;
            var profitTotal = currentState.PriceInUsd + currentState.ReceivedCouponInUsd - fullPriceBefore;
            var totalStr = $"Результат в usd: {currentState.PriceInUsd:F} - {assetOnPeriodData.Before.PriceInUsd:F} - {currentState.OperationFeesInUsd:F}";
            if (currentState.ReceivedCouponInUsd > 0)
                totalStr += $" + {currentState.ReceivedCouponInUsd:F}";
            result.Add(totalStr);
            result.Add($"                        = {profitTotal:N} usd ({profitTotal / assetOnPeriodData.Before.PriceInUsd * 100:F1}%)");
            result.Add("");
            return result;
        }

        private static List<string> FillAllChanges(AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var fromState = assetOnPeriodData.Before;
            var currentState = assetOnPeriodData.AtEnd;
            result.Add($"   цена бумаги {GetChangesLineForTwoFigures(fromState.PriceWithoutCoupon / fromState.Quantity, currentState.CurrentAssetRate, true)}");
            if (currentState.Asset.AssetType == AssetType.Bond)
            {
                var lineWithCoupon = GetChangesLineForTwoFigures(
                    fromState.AveragePrice,
                    currentState.AveragePrice + currentState.ReceivedCoupon / fromState.Quantity, true);
                result.Add($"     (с учетом НКД {lineWithCoupon})");
            }

            result.AddRange(GetChangesOfPackage(fromState, currentState));
            return result;
        }

        private static List<string> FillPeriodChanges(AssetState fromState, AssetState currentState)
        {
            List<string> result = new List<string>();
            result.AddRange(GetChangesOfAsset(fromState, currentState));
            result.AddRange(GetChangesOfPackage(fromState, currentState));
            return result;
        }

        private static List<string> GetChangesOfAsset(AssetState fromState, AssetState currentState)
        {
            List<string> result = new List<string>();
            result.Add($"   цена бумаги {GetChangesLineForTwoFigures(fromState.CurrentAssetRate, currentState.CurrentAssetRate, true)}");
            if (currentState.Asset.AssetType == AssetType.Bond)
            {
                var lineWithCoupon = GetChangesLineForTwoFigures(
                    fromState.CurrentAssetRate + fromState.AccumulatedCouponIncome,
                    currentState.CurrentAssetRate + currentState.AccumulatedCouponIncome, true);
                result.Add($"     (с учетом НКД {lineWithCoupon})");
            }
            return result;
        }

        private static List<string> GetChangesOfPackage(AssetState fromState, AssetState currentState)
        {
            List<string> result = new List<string>();
            result.Add($"   цена пакета {GetChangesLineForTwoFigures(fromState.Price, currentState.Price, true)}");
            if (currentState.Asset.TrustAccount.Currency != CurrencyCode.USD)
                result.Add($"   цена пакета в usd {GetChangesLineForTwoFigures(fromState.PriceInUsd, currentState.PriceInUsd, true)}");
            return result;
        }

        private static string GetChangesLineForTwoFigures(decimal a, decimal b, bool theMoreTheBetter)
        {
            if (a == 0) return "";
            var diff = theMoreTheBetter ? b - a : a - b;
            return $"{a:N} -> {b:N}  {diff / a * 100:F1}%";
        }

    }
}
