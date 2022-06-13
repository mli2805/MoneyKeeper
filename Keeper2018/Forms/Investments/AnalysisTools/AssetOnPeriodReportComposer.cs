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

            yield return $"Количество: {assetState.Quantity} шт.";
            yield return $" по цене: {assetState.AveragePriceStr}";
            yield return $"Стоимость: {assetState.PriceStr}";
            yield return "";

            yield return $"Оплачены комиссии {assetState.OperationFeesStr}";
            yield return "  т.е. увеличены:";
            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd;
            var averagePrice = assetState.Quantity == 0 ? 0 : fullPrice / assetState.Quantity;
            yield return $" средняя цена: {averagePrice:N} usd";
            yield return $" и полная стоимость: {fullPrice:N} usd";
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
    }

    public static class AssetOnPeriodReportComposer
    {
        public static AssetOnPeriodReportModel CreateReport(this AssetOnPeriodData assetOnPeriodData)
        {
            return new AssetOnPeriodReportModel()
            {
                BeforeState = assetOnPeriodData.Before.CreateBeforeColumn().ToList(),

                OnStartState = assetOnPeriodData.CreateOnStartColumn().ToList(),


                InBetweenTrans = CreateTraffic(assetOnPeriodData.InBetween).ToList(),
                InBetweenFeesAndCoupons = CreatePartB(assetOnPeriodData.InBetween, 2),

                AtEndState = CreateState(assetOnPeriodData.AtEnd).ToList(),
                AtEndFeesAndCoupons = CreatePartB(assetOnPeriodData.AtEnd, 3),
                AtEndAnalysis = CreateAnalysis(assetOnPeriodData.OnStart, assetOnPeriodData.AtEnd),
            };
        }


        private static IEnumerable<string> CreateState(AssetState assetState)
        {
            yield return assetState.Caption;
            yield return "";
            yield return $"Количество: {assetState.Quantity} шт.";
            yield return $" по цене: {assetState.AveragePriceStr}";
            yield return $"Стоимость: {assetState.PriceStr}";
        }

        private static IEnumerable<string> CreateTraffic(AssetState assetState)
        {
            yield return assetState.Caption;
            yield return "";
        }

        private static List<string> CreatePartB(AssetState assetState, int column)
        {
            var result = new List<string>();

            switch (column)
            {
                case 0:
                    {
                        if (assetState.OperationFees != 0)
                        {
                            result.Add($"Оплачены комиссии {assetState.OperationFeesStr}");
                            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd;
                            result.AddRange(CreatePartPlus(fullPrice, assetState.Quantity));
                        }
                        break;
                    }
                case 1:
                    {
                        if (assetState.ReceivedCoupon != 0)
                        {
                            result.Add($"Получен купон (дивы): {assetState.ReceivedCouponStr}");
                            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd - assetState.ReceivedCouponInUsd;
                            result.AddRange(CreatePartPlus(fullPrice, assetState.Quantity));
                        }
                        break;
                    }
                case 3:
                    {
                        if (assetState.OperationFees != 0)
                            result.Add($"Оплачены комиссии {assetState.OperationFeesStr}");
                        if (assetState.ReceivedCoupon != 0)
                            result.Add($"Получен купон (дивы): {assetState.ReceivedCouponStr}");
                        if (assetState.OperationFees != 0 || assetState.ReceivedCoupon != 0)
                        {
                            var fullPrice = assetState.PriceInUsd + assetState.OperationFeesInUsd - assetState.ReceivedCouponInUsd;
                            result.AddRange(CreatePartPlus(fullPrice, assetState.Quantity));
                        }
                        break;
                    }
                case 2:
                    {
                        if (assetState.OperationFees != 0)
                            result.Add($"Оплачены комиссии {assetState.OperationFeesStr}");
                        if (assetState.ReceivedCoupon != 0)
                            result.Add($"Получен купон (дивы): {assetState.ReceivedCouponStr}");
                        break;
                    }
            }

            return result;
        }

        private static IEnumerable<string> CreatePartPlus(decimal fullPrice, int quantity)
        {
            yield return "";
            yield return "Таким отбразом:";
            var averagePrice = quantity == 0 ? 0 : fullPrice / quantity;
            yield return $" средняя цена: {averagePrice:N} usd";
            yield return $"Полная стоимость: {fullPrice:N} usd";
        }

        private static AssetAnalysisModel CreateAnalysis(AssetState aState, AssetState bState)
        {
            var result = new AssetAnalysisModel();
            result.Content.Add(bState.Caption);

            var profit = bState.Price - aState.Price;
            result.ProfitInUsd = (bState.PriceInUsd + bState.ReceivedCouponInUsd)
                - (aState.PriceInUsd + aState.OperationFeesInUsd);
            var word = result.ProfitInUsd > 0 ? "прибыль составляла: " : "убыток составлял";

            var currency = bState.Asset.TrustAccount.Currency;
            var figures = currency == CurrencyCode.USD
                ? $"{result.ProfitInUsd:N} usd"
                : $"{profit:N} { currency.ToString().ToLower()} ({result.ProfitInUsd:N} usd)";
            result.Content.Add($"{word} {figures}");
            return result;
        }

    }
}
