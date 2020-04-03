using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class DepositReportModelExt
    {
        public static void SummarizeFacts(this DepositReportModel model, KeeperDb db)
        {
            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Contribution))
            {
                model.Contribution.AddBalance(reportLine.Income);
                model.ContributionUsd += db.BalanceInUsd(reportLine.Date, reportLine.Income);
            }

            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Revenue))
            {
                model.Revenue.AddBalance(reportLine.Income);
                model.RevenueUsd += db.BalanceInUsd(reportLine.Date, reportLine.Income);
            }

            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Consumption))
            {
                model.Сonsumption.AddBalance(reportLine.Outcome);
                model.ConsumptionUsd += db.BalanceInUsd(reportLine.Date, reportLine.Outcome);
            }

            model.DevaluationUsd = db.BalanceInUsd(DateTime.Today.GetEndOfDate(), model.Balance)
                                   - model.ContributionUsd - model.RevenueUsd + model.ConsumptionUsd;
            model.FinResultUsd = model.RevenueUsd + model.DevaluationUsd;
        }

        public static void Foresee(this DepositReportModel model, KeeperDb db)
        {
            ForeseeRate(model, db);
            ForeseeRevenue(model);

            model.BalanceAtEndUsd = model.IsInUsd
                ? model.Balance.Currencies[CurrencyCode.USD]
                : model.Balance.Currencies[model.DepositOffer.MainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : model.DepositOffer.MainCurrency] 
                  / (decimal)model.RateForecast.Value;
            model.ForecastUsd = model.BalanceAtEndUsd + model.MoreRevenueUsd - model.ContributionUsd + model.ConsumptionUsd;

            var procent = model.ForecastUsd / model.ContributionUsd * 100;
            var yearProcent = procent / (model.Deposit.FinishDate - model.Deposit.StartDate).Days * 365;
            model.Forecast2 = $"( {procent:0.00}%  ({yearProcent:0.00}% годовых )";
        }

        private static void ForeseeRevenue(DepositReportModel model)
        {
            var lastRevenue = model.Traffic.LastOrDefault(t => t.Type == DepositOperationType.Revenue);
            var lastRevenueDate = lastRevenue?.Date ?? model.Deposit.StartDate.AddDays(-1);

            model.MoreRevenue = model.DepositOffer.GetRevenueUptoDepoFinish(model.Deposit, lastRevenueDate,
                model.Balance.Currencies[model.DepositOffer.MainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : model.DepositOffer.MainCurrency]);
            model.MoreRevenueUsd = model.IsInUsd
                ? model.MoreRevenue
                : model.MoreRevenue / (decimal)((model.RateNow.Value + model.RateForecast.Value) / 2);
        }

        private static void ForeseeRate(DepositReportModel model, KeeperDb db)
        {
            if (model.IsInUsd) return;

            if (model.DepositOffer.MainCurrency == CurrencyCode.BYN ||
                model.DepositOffer.MainCurrency == CurrencyCode.BYR)
            {
                model.RateStart = db.GetRate(model.Deposit.StartDate, model.DepositOffer.MainCurrency);
                if (model.Deposit.StartDate < new DateTime(2016, 7, 1))
                    model.RateStart.Value = model.RateStart.Value / 10000;
                model.RateNow = db.GetRate(DateTime.Today, model.DepositOffer.MainCurrency);
            }
            else
            {
                model.RateStart = db.GetRate(model.Deposit.StartDate, model.DepositOffer.MainCurrency, true);
                model.RateNow = db.GetRate(DateTime.Today, model.DepositOffer.MainCurrency, true);
            }

            model.RateForecast = new OneRate();
            model.RateForecast.Unit = model.RateNow.Unit;
            var k = (model.RateNow.Value - model.RateStart.Value) / (DateTime.Today - model.Deposit.StartDate).Days;
            model.RateForecast.Value = model.RateStart.Value + k * (model.Deposit.FinishDate - model.Deposit.StartDate).Days;

            model.RateStr1 =
                $"Курс по НБ:  {model.RateStart.Value:0.0000} => {model.RateNow.Value:0.0000} => {model.RateForecast.Value:0.0000}";

            var nowDeltaRate = model.RateNow.Value - model.RateStart.Value;
            var nowProcent = nowDeltaRate / model.RateStart.Value * 100;

            var deltaRate = model.RateForecast.Value - model.RateStart.Value;
            var procent = deltaRate / model.RateStart.Value * 100;

            var yearProcent = procent / (model.Deposit.FinishDate - model.Deposit.StartDate).Days * 365;
            model.RateStr2 = $"( уже {nowProcent:0.00}%;  к концу {procent:0.00}%  ( {yearProcent:0.00}% годовых )";
        }
    }
}