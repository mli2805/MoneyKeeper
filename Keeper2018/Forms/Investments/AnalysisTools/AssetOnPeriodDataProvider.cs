using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AssetOnPeriodDataProvider
    {
        public static AssetOnPeriodData Analyze(this KeeperDataModel dataModel, TrustAssetModel asset, Period period)
        {
            var result = new AssetOnPeriodData() { Period = period };

            // цена и колво (и курсы если не долларовый) на дату последней операции до начала периода
            // а также средняя цена единицы актива
            // и справочно транзакции до начала периода
            var lastDayBefore = period.StartDate.AddMilliseconds(-1);
            var periodBefore = new Period(DateTime.MinValue, lastDayBefore);

            var state = new AssetState(asset);
            result.Before = dataModel.GetAssetOnDate(state, periodBefore, @"Ранее");

            // переоцениваем на начало периода
            result.OnStart = dataModel.RecalculateOnDate(result.Before, lastDayBefore, $"На {period.StartDate.AddSeconds(-1)}");

            var finish = period.StartDate.Date <= DateTime.Today && period.FinishMoment.Date > DateTime.Today
                ? DateTime.Today
                : period.FinishMoment;

            // только транзакции в течении периода
            var emptyState = new AssetState(asset);
            //result.InBetween = dataModel.GetAssetOnDate(emptyState, period, period.ToStringD());
            result.InBetween = dataModel.GetAssetOnDate(emptyState, period, $"{period.StartDate.Date:d} - {finish:d}");

            // цена и колво (и курсы если не долларовый) на дату последней операции внутри периода
            // переоцениваем на конец периода
            var temp = dataModel.GetAssetOnDate(result.OnStart, period, "");
            result.AtEnd = dataModel.RecalculateOnDate(temp, period.FinishMoment.Date, $"На {finish:d}");

            // накопленный купонный доход
            // выплаченный купонный доход или дивиденды

            // изменение стоимости актива (изменение с учетом курса) за период 
            // изменение стоимости актива (изменение с учетом курса) с момента покупки
            // 

            return result;
        }

        private static AssetState GetAssetOnDate(this KeeperDataModel dataModel, AssetState stateBefore,
            Period period, string caption)
        {
            var result = stateBefore.Clone(caption);

            var isInUsd = result.Asset.TrustAccount.Currency == CurrencyCode.USD;

            var trans = dataModel.InvestTranModels
                .Where(t => t.Asset.Ticker == result.Asset.Ticker
                            && period.Includes(t.Timestamp))
                .ToList();
            result.Trans.AddRange(trans);

            foreach (var investTranModel in trans)
            {
                decimal rate = 1;
                if (!isInUsd)
                    rate = (decimal)dataModel.GetRate(investTranModel.Timestamp.Date, investTranModel.Currency, true).Value;
                investTranModel.Rate = rate;

                var feeCurrencyRate = (decimal)dataModel
                    .GetRate(investTranModel.Timestamp.Date, investTranModel.BuySellFeeCurrency, true).Value;

                switch (investTranModel.InvestOperationType)
                {
                    case InvestOperationType.BuyBonds:
                    case InvestOperationType.BuyStocks:
                        result.Quantity += investTranModel.AssetAmount;
                        result.PriceWithoutCoupon += investTranModel.CurrencyAmount;
                        result.Price += investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                        result.PriceInUsd += (investTranModel.CurrencyAmount + investTranModel.CouponAmount) / rate;

                        result.OperationFees += investTranModel.BuySellFee;
                        result.OperationFeesInUsd += investTranModel.BuySellFee / feeCurrencyRate;
                        break;
                    case InvestOperationType.SellBonds:
                    case InvestOperationType.SellStocks:
                        result.Quantity -= investTranModel.AssetAmount;
                        result.PriceWithoutCoupon -= investTranModel.CurrencyAmount;
                        result.Price -= investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                        result.PriceInUsd -= (investTranModel.CurrencyAmount + investTranModel.CouponAmount) / rate;

                        result.OperationFees += investTranModel.BuySellFee;
                        result.OperationFeesInUsd += investTranModel.BuySellFee / feeCurrencyRate;
                        break;
                    case InvestOperationType.EnrollCouponOrDividends:
                        result.ReceivedCoupon += investTranModel.CurrencyAmount;
                        result.ReceivedCouponInUsd += investTranModel.CurrencyAmount / rate;
                        break;
                }

            }

            if (!isInUsd)
                result.CurrentCurrencyRate =
                    (decimal)dataModel.GetRate(period.FinishMoment.Date, result.Asset.TrustAccount.Currency, true).Value;
            return result;
        }

        private static AssetState RecalculateOnDate(this KeeperDataModel dataModel,
            AssetState state, DateTime date, string caption)
        {
            var result = state.Clone(caption);
            var asset = state.Asset;

            result.CurrentAssetRate = dataModel.AssetRates.LastOrDefault(r => r.TickerId == asset.Id && r.Date <= date)?.Value ?? 0;
            result.AccumulatedCouponIncome = asset.GetAccumulatedCoupon(date);

            result.Price = (result.CurrentAssetRate + result.AccumulatedCouponIncome) * state.Quantity;

            if (asset.TrustAccount.Currency == CurrencyCode.RUB)
            {
                var rubToUsd = dataModel.GetExchangeRatesLine(date).RubToUsd;
                result.CurrentCurrencyRate = (decimal)rubToUsd;
                result.CurrentCurrencyRateStr = $"   курс: {rubToUsd} rub купить 1 usd";
                result.PriceInUsd = result.Price / (decimal)rubToUsd;
            }
            else result.PriceInUsd = result.Price;

            return result;
        }
    }

    public static class InvestmentAssetExt
    {
        public static decimal GetAccumulatedCoupon(this TrustAssetModel asset, DateTime date)
        {
            var days = (date - asset.PreviousCouponDate).Days;
            return asset.Nominal * (decimal)asset.CouponRate / 100 * days / 365;
        }
    }
}