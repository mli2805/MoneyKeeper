using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class InvestmentAnalysis
    {
        private static InvestmentAssetState GetAssetOnDate(this KeeperDataModel dataModel, InvestmentAssetState stateBefore,
            Period period)
        {
            var result = new InvestmentAssetState(stateBefore);

            var trans = dataModel.InvestTranModels
                .Where(t => t.Asset.Ticker == stateBefore.Asset.Ticker && period.Includes(t.Timestamp))
                .ToList();
            result.Trans.AddRange(trans);

            foreach (var investTranModel in trans)
            {
                switch (investTranModel.InvestOperationType)
                {
                    case InvestOperationType.BuyBonds:
                    case InvestOperationType.BuyStocks:
                        result.Quantity += investTranModel.AssetAmount;
                        result.Price += investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                        break;
                    case InvestOperationType.SellBonds:
                    case InvestOperationType.SellStocks:
                        result.Quantity -= investTranModel.AssetAmount;
                        result.Price -= investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                        break;
                }
            }

            return result;
        }

        private static InvestmentAssetState RecalculateOnDate(this KeeperDataModel dataModel,
            InvestmentAssetState state, DateTime date)
        {
            var result = new InvestmentAssetState(state.Asset) { Quantity = state.Quantity };

            var assetRate = dataModel.AssetRates.Last(r => r.TickerId == state.Asset.Id && r.Date <= date);
            result.Price = assetRate.Value;

            if (assetRate.Currency != CurrencyCode.USD)
            {
                var usdRate = dataModel.GetRate(date, assetRate.Currency);
                result.PriceInUsd = assetRate.Value * (decimal)usdRate.Value;
            }
            else result.PriceInUsd = result.Price;

            return result;
        }

        public static InvestmentAssetOnPeriod Analyze(this KeeperDataModel dataModel, InvestmentAsset asset, Period period)
        {
            var result = new InvestmentAssetOnPeriod();


            // цена и колво (и курсы если не долларовый) на начало периода
            // а также средняя цена единицы актива
            // ( с учетом покупок и продаж до начала периода, а также комиссии за операции)
            // и справочно транзакции до начала периода

            var lastDayBefore = period.StartDate.AddMilliseconds(-1);
            result.Initial = dataModel
                 .GetAssetOnDate(new InvestmentAssetState(asset),
                         new Period(DateTime.MinValue, lastDayBefore));
            result.Start = dataModel.RecalculateOnDate(result.Initial, lastDayBefore);

            // цена и колво (и курсы если не долларовый) на конец периода
            // и справочно транзакции в течении периода

            result.End = dataModel.GetAssetOnDate(result.Initial, period);

            // накопленный купонный доход
            // выплаченный купонный доход или дивиденды

            // изменение стоимости актива (изменение с учетом курса) за период 
            // изменение стоимости актива (изменение с учетом курса) с момента покупки
            // 

            return result;
        }
    }
}