using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class BeforeColumnComposer
    {
        public static IEnumerable<string> CreateBeforeColumn(this AssetState assetState)
        {
            yield return assetState.Caption;
            yield return "";

            foreach (var tran in assetState.Trans
                         .Where(t => t.InvestOperationType == InvestOperationType.BuyBonds
                                               || t.InvestOperationType == InvestOperationType.BuyStocks
                                               || t.InvestOperationType == InvestOperationType.SellBonds
                                               || t.InvestOperationType == InvestOperationType.SellStocks))
                yield return tran.ToAssetAnalysis();
            yield return "";

            yield return $"Количество: {assetState.Quantity} шт.";
            yield return $" средняя цена: {assetState.AveragePriceStr}";
            yield return $" cтоимость: {assetState.PriceStr}";
            yield return "";

            yield return "С учетом оплаченных";
            yield return $" комиссий {assetState.OperationFeesStr}";
            yield return "получились:";
            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            yield return $" средняя цена: {averagePrice:N} usd";
            yield return $" и стоимость: {fullPrice:N} usd";
        }

        public static IEnumerable<string> CreateOnStartColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            var assetStateBefore = assetOnPeriodData.Before;
            var assetState = assetOnPeriodData.OnStart;
            yield return assetState.Caption;
            yield return "";

            yield return $"Количество: {assetState.Quantity} шт.";
            yield return $" по цене: {assetState.AveragePriceStr}";
            yield return $"Стоимость: {assetState.PriceStr}";
            yield return "";

            var fullPrice = assetStateBefore.PriceInUsd + assetState.OperationFeesInUsd - assetState.ReceivedCouponInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            if (assetState.ReceivedCouponInUsd > 0)
            {
                yield return $"Получен купон (дивы): {assetState.ReceivedCouponStr}";
                yield return "  можно сказать уменьшены:";
                yield return $" средняя цена: {averagePrice:N} usd";
                yield return $" и стоимость покупки: {fullPrice:N} usd";
                yield return "";
            }


            yield return assetState.Caption;
            var profitInUsd = assetState.PriceInUsd - fullPrice;
            var word = profitInUsd > 0 ? "прибыль составляла: " : "убыток составлял";
            yield return $"{word} {profitInUsd:N} usd";
        }

        public static IEnumerable<string> CreateInBetweenColumn(this AssetState assetState)
        {
            yield return assetState.Caption;
            yield return "";

            foreach (var tran in assetState.Trans
                         .Where(t => t.InvestOperationType == InvestOperationType.BuyBonds
                                               || t.InvestOperationType == InvestOperationType.BuyStocks
                                               || t.InvestOperationType == InvestOperationType.SellBonds
                                               || t.InvestOperationType == InvestOperationType.SellStocks))
                yield return tran.ToAssetAnalysis();
            yield return "";

            if (assetState.OperationFees != 0)
                yield return $"Оплачены комиссии {assetState.OperationFeesStr}";
            if (assetState.ReceivedCoupon != 0)
                yield return $"Получен купон (дивы): {assetState.ReceivedCouponStr}";

        }

        public static IEnumerable<string> CreateAtEndColumn(this AssetOnPeriodData assetOnPeriodData)
        {
            var assetStateBefore = assetOnPeriodData.OnStart;
            var assetState = assetOnPeriodData.AtEnd;
            yield return assetState.Caption;
            yield return "";

            yield return $"Количество: {assetState.Quantity} шт.";
            yield return $" по цене: {assetState.AveragePriceStr}";
            yield return $"Стоимость: {assetState.PriceStr}";
            yield return "";

            var fullPrice = assetStateBefore.PriceInUsd + assetState.OperationFeesInUsd - assetState.ReceivedCouponInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            if (assetState.ReceivedCouponInUsd > 0)
            {
                yield return "С учетом оплаченных";
                yield return $" комиссий {assetState.OperationFeesStr}";
                yield return $"и зачисленных купон (дивы): {assetState.ReceivedCouponStr}";
                yield return "  получились:";
                yield return $" средняя цена: {averagePrice:N} usd";
                yield return $" и стоимость покупки: {fullPrice:N} usd";
                yield return "";
            }

            yield return "здесь будет вычисляться прибыль/убыток";
            yield return "";


        }



        private static string ToAssetAnalysis(this InvestTranModel tranModel)
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

            var figures = $"{tranModel.AssetAmount} * {oneAssetPrice} = {sum:N}";
            return $"{tranModel.Timestamp:dd-MMM-yy} {word} {figures} {tranModel.Currency.ToString().ToLower()}";
        }
    }

    public static class AssetOnPeriodReportComposer
    {
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
