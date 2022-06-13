using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AssetOnPeriodDataProvider
    {
        public static AssetOnPeriodData Analyze(this KeeperDataModel dataModel, InvestmentAssetModel asset, Period period)
        {
            var result = new AssetOnPeriodData(){Period = period};

            // цена и колво (и курсы если не долларовый) на дату последней операции до начала периода
            // а также средняя цена единицы актива
            // и справочно транзакции до начала периода
            var lastDayBefore = period.StartDate.AddMilliseconds(-1);
            var periodBefore = new Period(DateTime.MinValue, lastDayBefore);

            var state = new AssetState(asset);
            result.Before = dataModel.GetAssetOnDate(state, periodBefore, @"Ранее");

            // переоцениваем на начало периода
            result.OnStart = dataModel.RecalculateOnDate(result.Before, lastDayBefore, $"На {period.StartDate.AddSeconds(-1)}");

            // только транзакции в течении периода
            var emptyState = new AssetState(asset);
            result.InBetween = dataModel.GetAssetOnDate(emptyState, period, period.ToStringD());

            // цена и колво (и курсы если не долларовый) на дату последней операции внутри периода
            // переоцениваем на конец периода
            var temp = dataModel.GetAssetOnDate(result.OnStart, period, "");
            result.AtEnd = dataModel.RecalculateOnDate(temp, period.FinishMoment.Date, $"На {period.FinishMoment}");

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
                .Where(t => t.Asset.Ticker == result.Asset.Ticker && period.Includes(t.Timestamp))
                .ToList();
            result.Trans.AddRange(trans);

            foreach (var investTranModel in trans)
            {
                decimal rate = 1;
                if (!isInUsd)
                    rate = (decimal)dataModel.GetRate(investTranModel.Timestamp.Date, investTranModel.Currency, true).Value;

                var feeCurrencyRate = (decimal)dataModel
                    .GetRate(investTranModel.Timestamp.Date, investTranModel.BuySellFeeCurrency, true).Value;

                switch (investTranModel.InvestOperationType)
                {
                    case InvestOperationType.BuyBonds:
                    case InvestOperationType.BuyStocks:
                        result.Quantity += investTranModel.AssetAmount;
                        result.Price += investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                        result.PriceInUsd += (investTranModel.CurrencyAmount + investTranModel.CouponAmount) / rate;

                        result.OperationFees += investTranModel.BuySellFee;
                        result.OperationFeesInUsd += investTranModel.BuySellFee / feeCurrencyRate;
                        break;
                    case InvestOperationType.SellBonds:
                    case InvestOperationType.SellStocks:
                        result.Quantity -= investTranModel.AssetAmount;
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

            return result;
        }

        private static AssetState RecalculateOnDate(this KeeperDataModel dataModel,
            AssetState state, DateTime date, string caption)
        {
            var result = state.Clone(caption);

            var assetRate = dataModel.AssetRates.LastOrDefault(r => r.TickerId == state.Asset.Id && r.Date <= date);
            if (assetRate == null)
                return result;

            result.Price = assetRate.Value * state.Quantity;

            if (assetRate.Currency != CurrencyCode.USD)
            {
                var usdRate = dataModel.GetRate(date, assetRate.Currency, true);
                result.PriceInUsd = assetRate.Value / (decimal)usdRate.Value * state.Quantity;
            }
            else result.PriceInUsd = result.Price;

            return result;
        }
    }
}