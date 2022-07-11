using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AssetOnPeriodReportComposer
    {
        public static List<string> CreateBeforeColumn(this AssetState assetState)
        {
            List<string> result = new List<string>();
            result.Add(assetState.Caption);
            result.Add("");

            foreach (var tran in assetState.Trans
                         .Where(t => t.InvestOperationType.IsBuySell()))
                result.AddRange(tran.ToAssetAnalysis());
            result.Add("");

            result.Add($"Количество: {assetState.Quantity} шт.");
            result.Add($" средняя цена: {assetState.AveragePriceStr}");
            result.Add($" cтоимость: {assetState.PriceStr}");
            result.Add("");

            result.Add("С учетом оплаченных");
            result.Add($" комиссий {assetState.OperationFeesStr}");
            result.Add("получились:");
            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            result.Add($" средняя цена: {averagePrice:N} usd");
            result.Add($" и стоимость: {fullPrice:N} usd");
            return result;
        }

        public static IEnumerable<string> CreateOnStartColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var assetStateBefore = assetOnPeriodData.Before;
            var assetState = assetOnPeriodData.OnStart;
            result.Add(assetState.Caption);
            result.Add("");

            result.Add($"Количество: {assetState.Quantity} шт.");
            result.Add($" по цене: {assetState.AveragePriceStr}");
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

        public static IEnumerable<string> CreateInBetweenColumn(this AssetState assetState)
        {
            List<string> result = new List<string>();
            result.Add(assetState.Caption);
            result.Add("");

            foreach (var tran in assetState.Trans
                         .Where(t => t.InvestOperationType == InvestOperationType.BuyBonds
                                               || t.InvestOperationType == InvestOperationType.BuyStocks
                                               || t.InvestOperationType == InvestOperationType.SellBonds
                                               || t.InvestOperationType == InvestOperationType.SellStocks))
                result.AddRange(tran.ToAssetAnalysis());
            result.Add("");

            if (assetState.OperationFees != 0)
                result.Add($"Оплачены комиссии {assetState.OperationFeesStr}");
            if (assetState.ReceivedCoupon != 0)
                result.Add($"Получен купон (дивы): {assetState.ReceivedCouponStr}");
            return result;
        }

        public static IEnumerable<string> CreateAtEndColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            List<string> result = new List<string>();
            var assetStateBefore = assetOnPeriodData.OnStart;
            var assetState = assetOnPeriodData.AtEnd;
            result.Add(assetState.Caption);
            result.Add("");

            result.Add($"Количество: {assetState.Quantity} шт.");
            result.Add($" по цене: {assetState.AveragePriceStr}");
            result.Add($"Стоимость: {assetState.PriceStr}");
            result.Add("");

            var fullPrice = assetStateBefore.PriceInUsd + assetState.OperationFeesInUsd - assetState.ReceivedCouponInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            if (assetState.ReceivedCouponInUsd > 0)
            {
                result.Add("С учетом оплаченных");
                result.Add($" комиссий {assetState.OperationFeesStr}");
                result.Add($"и зачисленных купон (дивы): {assetState.ReceivedCouponStr}");
                result.Add("  получились:");
                result.Add($" средняя цена: {averagePrice:N} usd");
                result.Add($" и стоимость покупки: {fullPrice:N} usd");
                result.Add("");
            }

            result.Add("здесь будет вычисляться прибыль/убыток");
            result.Add("");
            return result;
        }

        private static IEnumerable<string> ToAssetAnalysis(this InvestTranModel tranModel)
        {
            string word;
            if (tranModel.InvestOperationType == InvestOperationType.BuyStocks
               || tranModel.InvestOperationType == InvestOperationType.BuyBonds)
                word = "купил";
            else if (tranModel.InvestOperationType == InvestOperationType.SellStocks
                || tranModel.InvestOperationType == InvestOperationType.SellBonds)
                word = "продал";
            else word = "not implemented yet";

            decimal sum;
            string oneAssetPrice;
            if (tranModel.Asset.AssetType == AssetType.Bond)
            {
                sum = tranModel.CurrencyAmount + tranModel.CouponAmount;
                oneAssetPrice = $"({tranModel.CurrencyAmount / tranModel.AssetAmount:N} + {tranModel.CouponAmount / tranModel.AssetAmount:N})";
            }
            else
            {
                sum = tranModel.CurrencyAmount;
                oneAssetPrice = $"{tranModel.CurrencyAmount / tranModel.AssetAmount:N}";
            }

            var inCurrency = $"{tranModel.AssetAmount} * {oneAssetPrice} = {sum:N} {tranModel.Currency.ToString().ToLower()}";
            if (tranModel.Asset.TrustAccount.Currency == CurrencyCode.USD)
                yield return $"{tranModel.Timestamp:dd-MMM-yy} {word} {inCurrency}";
            else
            {
                var inUsd = $" ({sum / tranModel.Rate:N} usd)";
                yield return $"{tranModel.Timestamp:dd-MMM-yy} {word}";
                yield return $"         {inCurrency} {inUsd}";
            }
        }

        public static AssetOnPeriodReportModel CreateReport(this AssetOnPeriodData assetOnPeriodData)
        {
            return new AssetOnPeriodReportModel()
            {
                BeforeState = assetOnPeriodData.Before.CreateBeforeColumn().ToList(),

                OnStartState = assetOnPeriodData.CreateOnStartColumn().ToList(),

                InBetweenTrans = assetOnPeriodData.InBetween.CreateInBetweenColumn().ToList(),

                AtEndState = assetOnPeriodData.CreateAtEndColumn().ToList(),
            };
        }

    }
}
