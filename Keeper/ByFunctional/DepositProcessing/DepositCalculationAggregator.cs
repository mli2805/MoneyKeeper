using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class DepositCalculationAggregator
    {
        private readonly DepositCalculator _depositCalculator;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public DepositCalculationAggregator(DepositCalculator depositCalculator, RateExtractor rateExtractor)
        {
            _depositCalculator = depositCalculator;
            _rateExtractor = rateExtractor;
        }

        public void FillinFieldsForOneDepositReport(Deposit deposit)
        {
            _depositCalculator.Calculate(deposit);
            CalculateMonthEstimatedProcents(deposit, DateTime.Today);
            CalculateUpToEndEstimatedProcents(deposit);
        }

        public void FillinFieldsForMonthAnalysis(Deposit deposit, DateTime day)
        {
            _depositCalculator.Calculate(deposit);
            CalculateMonthEstimatedProcents(deposit, day);
        }

        private void CalculateMonthEstimatedProcents(Deposit deposit, DateTime firstDayOfAnalyzedMonth)
        {
            var periodWhichShouldBePaidInAnalysedMonth = deposit.GetPeriodWhichShouldBePaidInAnalysedMonth(firstDayOfAnalyzedMonth);
            deposit.CalculationData.Estimations = new DepositEstimations();
            deposit.CalculationData.Estimations.ProcentsInThisMonth = periodWhichShouldBePaidInAnalysedMonth.ShouldBePaid() ?
                deposit.CalculationData.DailyTable.Where(l => periodWhichShouldBePaidInAnalysedMonth.ContainsAndTimeWasChecked(l.Date)).Sum(l => l.DayProcents)
                : 0;
        }

        /// <summary>
        /// Предсказание по последним 30 дням
        /// </summary>
        /// <param name="deposit"></param>
        /// <param name="dateForPredicion"></param>
        private void CalculateCurrencyRateOnThisMonthPayment(Deposit deposit, DateTime dateForPredicion)
        {
            deposit.CalculationData.Estimations.CurrencyRateOnThisMonthPayment =
                deposit.CalculationData.DailyTable.First(l => l.Date == dateForPredicion).CurrencyRate;
        }


        private void MindDevaluation(Deposit deposit)
        {
            deposit.CalculationData.Estimations.CurrencyRateOnFinish = ForecastCurrencyRateOnFinish(deposit);
        }

        private decimal ForecastCurrencyRateOnFinish(Deposit deposit)
        {
            var currentRate = (decimal)_rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today);
            var delta = (currentRate - (decimal)_rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today.AddDays(-30))) / 30;
            return currentRate + (deposit.FinishDate - DateTime.Today).Days * delta;
        }

        private void CalculateUpToEndEstimatedProcents(Deposit deposit)
        {
            var periodFromLastProcentToEnd = new Period(deposit.GetDateOfLastProcentTransaction(), deposit.FinishDate);
            deposit.CalculationData.Estimations.ProcentsUpToFinish =
                deposit.CalculationData.DailyTable.Where(l => periodFromLastProcentToEnd.ContainsAndTimeWasChecked(l.Date)).Sum(l => l.DayProcents);

            if (deposit.DepositOffer.Currency != CurrencyCodes.USD) MindDevaluation(deposit);
        }

        public decimal GetProfitForYear(Deposit deposit, int year)
        {
            if (deposit.CalculationData.CurrentProfitInUsd == 0) return 0;
            var startYear = deposit.CalculationData.Traffic.First().Timestamp.Year;
            var finishYear = deposit.CalculationData.Traffic.Last().Timestamp.AddDays(-1).Year;
            if (year < startYear || year > finishYear) return 0;
            if (startYear == finishYear) return deposit.CalculationData.CurrentProfitInUsd;
            var allDaysCount = (deposit.CalculationData.Traffic.Last().Timestamp.AddDays(-1) - deposit.CalculationData.Traffic.First().Timestamp).Days;
            if (year == startYear)
            {
                var startYearDaysCount = (new DateTime(startYear, 12, 31) - deposit.CalculationData.Traffic.First().Timestamp).Days;
                return deposit.CalculationData.CurrentProfitInUsd * startYearDaysCount / allDaysCount;
            }
            if (year == finishYear)
            {
                var finishYearDaysCount = (deposit.CalculationData.Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
                return deposit.CalculationData.CurrentProfitInUsd * finishYearDaysCount / allDaysCount;
            }
            var yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
            return deposit.CalculationData.CurrentProfitInUsd * yearDaysCount / allDaysCount;
        }

    }
}
