using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class DepositReportModelExt
    {
        public static void SummarizeFacts(this DepositReportModel model, KeeperDataModel dataModel)
        {
            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Contribution))
            {
                model.Contribution.AddBalance(reportLine.Income);
                model.ContributionUsd += dataModel.BalanceInUsd(reportLine.Date, reportLine.Income);
            }

            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Revenue))
            {
                model.Revenue.AddBalance(reportLine.Income);
                model.RevenueUsd += dataModel.BalanceInUsd(reportLine.Date, reportLine.Income);
            }

            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Consumption))
            {
                model.Сonsumption.AddBalance(reportLine.Outcome);
                model.ConsumptionUsd += dataModel.BalanceInUsd(reportLine.Date, reportLine.Outcome);
            }

            model.DevaluationUsd = dataModel.BalanceInUsd(DateTime.Today.GetEndOfDate(), model.Balance)
                                   - model.ContributionUsd - model.RevenueUsd + model.ConsumptionUsd;
            model.FinResultUsd = model.RevenueUsd + model.DevaluationUsd;
        }

        public static void Foresee(this DepositReportModel model, KeeperDataModel dataModel)
        {
            ForeseeRate(model, dataModel);
            ForeseeRevenue(model, dataModel);

            model.BalanceAtEndUsd = model.IsInUsd
                ? model.Balance.Currencies[CurrencyCode.USD]
                : model.Balance.Currencies[model.BankAccount.MainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : model.BankAccount.MainCurrency] 
                  / (decimal)model.RateForecast.Value;
            model.ForecastUsd = model.BalanceAtEndUsd + model.MoreRevenueUsd - model.ContributionUsd + model.ConsumptionUsd;

            var procent = model.ForecastUsd / model.ContributionUsd * 100;
            var yearProcent = procent / (model.BankAccount.FinishDate - model.BankAccount.StartDate).Days * 365;
            model.Forecast2 = $"( {procent:0.00}%  ({yearProcent:0.00}% годовых )";
        }

        private static void ForeseeRevenue(DepositReportModel model, KeeperDataModel dataModel)
        {
            var lastRevenue = model.Traffic.LastOrDefault(t => t.Type == DepositOperationType.Revenue);
            var lastRevenueDate = lastRevenue?.Date ?? model.BankAccount.StartDate.AddDays(-1);

            var depositOffer = dataModel.DepositOffers.First(o => o.Id == model.BankAccount.DepositOfferId);
            model.MoreRevenue = GetRevenueUptoDepoFinish(model.BankAccount, depositOffer, lastRevenueDate,
                model.Balance.Currencies[model.BankAccount.MainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : model.BankAccount.MainCurrency]);
            model.MoreRevenueUsd = model.IsInUsd
                ? model.MoreRevenue
                : model.MoreRevenue / (decimal)((model.RateNow.Value + model.RateForecast.Value) / 2);
        }

        private static decimal GetRevenueUptoDepoFinish
            (BankAccountModel bankAccount, DepositOfferModel depositOffer, DateTime lastReceivedRevenueDate, decimal currentAmount)
        {
            var conditionses = depositOffer.CondsMap.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= bankAccount.StartDate).Value;

            var rateLines =
                conditionses.RateLines.Where(l => l.AmountFrom <= currentAmount && l.AmountTo >= currentAmount).
                    OrderBy(o => o.DateFrom).ToArray();

            decimal revenue = 0;
            int i;
            for (i = 0; i < rateLines.Length; i++)
            {
                if (rateLines[i].DateFrom > lastReceivedRevenueDate) break;
            }

            if (i == rateLines.Length) i--;

            var startPeriod = lastReceivedRevenueDate;
            for (int j = i; j < rateLines.Length; j++)
            {
                var endOfPeriod = bankAccount.FinishDate;
                if (rateLines.Length > j + 1 && rateLines[j + 1].DateFrom < endOfPeriod)
                    endOfPeriod = rateLines[j + 1].DateFrom;

                var days = (endOfPeriod - startPeriod).Days;
                revenue = revenue + currentAmount * rateLines[j].Rate / 100 * days / 365;
            }

            return revenue;
        }

        private static void ForeseeRate(DepositReportModel model, KeeperDataModel dataModel)
        {
            if (model.IsInUsd) return;

            if (model.BankAccount.MainCurrency == CurrencyCode.BYN ||
                model.BankAccount.MainCurrency == CurrencyCode.BYR)
            {
                model.RateStart = dataModel.GetRate(model.BankAccount.StartDate, model.BankAccount.MainCurrency);
                if (model.BankAccount.StartDate < new DateTime(2016, 7, 1))
                    model.RateStart.Value /= 10000;
                model.RateNow = dataModel.GetRate(DateTime.Today, model.BankAccount.MainCurrency);
            }
            else
            {
                model.RateStart = dataModel.GetRate(model.BankAccount.StartDate, model.BankAccount.MainCurrency, true);
                model.RateNow = dataModel.GetRate(DateTime.Today, model.BankAccount.MainCurrency, true);
            }

            model.RateForecast = new OneRate();
            model.RateForecast.Unit = model.RateNow.Unit;
            var k = (model.RateNow.Value - model.RateStart.Value) / (DateTime.Today - model.BankAccount.StartDate).Days;
            if (model.BankAccount.MainCurrency == CurrencyCode.RUB && k < 0) k = 0;
            model.RateForecast.Value = model.RateStart.Value + k * (model.BankAccount.FinishDate - model.BankAccount.StartDate).Days;

            model.RateStr1 = model.BankAccount.MainCurrency == CurrencyCode.RUB
                ? $"Курс $:  {model.RateStart.Value:0.0#} => {model.RateNow.Value:0.0#} => {model.RateForecast.Value:0.0#}"
                : $"Курс $:  {model.RateStart.Value:0.0000} => {model.RateNow.Value:0.0000} => {model.RateForecast.Value:0.0000}";

            var nowDeltaRate = model.RateNow.Value - model.RateStart.Value;
            var nowProcent = nowDeltaRate / model.RateStart.Value * 100;

            var deltaRate = model.RateForecast.Value - model.RateStart.Value;
            var procent = deltaRate / model.RateStart.Value * 100;

            var yearProcent = procent / (model.BankAccount.FinishDate - model.BankAccount.StartDate).Days * 365;
            model.RateStr2 = $"( уже {nowProcent:0.00}%;  к концу {procent:0.00}%  ( {yearProcent:0.00}% годовых )";
        }
    }
}