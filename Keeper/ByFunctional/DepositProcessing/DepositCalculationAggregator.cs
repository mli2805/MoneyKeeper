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
            if (deposit.CalculationData.State == DepositStates.Закрыт) return;
            CalculateMonthEstimatedProcents(deposit, DateTime.Today);
            CalculateUpToEndEstimatedProcents(deposit);
        }

        public void FillinFieldsForMonthAnalysis(Deposit deposit, DateTime day)
        {
            _depositCalculator.Calculate(deposit);
            CalculateMonthEstimatedProcents(deposit, day);
        }

        private static void CalculateMonthEstimatedProcents(Deposit deposit, DateTime firstDayOfAnalyzedMonth)
        {
            deposit.CalculationData.Estimations.PeriodForThisMonthPayment = deposit.GetPeriodWhichShouldBePaidInAnalysedMonth(firstDayOfAnalyzedMonth);
            if (deposit.CalculationData.Estimations.PeriodForThisMonthPayment.ShouldBePaid())
            {
                deposit.CalculationData.Estimations.ProcentsInThisMonth =
                    deposit.CalculationData.DailyTable.Where(
                        l => deposit.CalculationData.Estimations.PeriodForThisMonthPayment.ContainsButTimeNotChecking(l.Date))
                        .Sum(l => l.DayProcents);
                deposit.CalculationData.Estimations.CurrencyRateOnThisMonthPayment =
                    deposit.CalculationData.DailyTable.First(l => l.Date.Date == deposit.CalculationData.Estimations.PeriodForThisMonthPayment.Finish.Date).CurrencyRate;

            }
            else deposit.CalculationData.Estimations.ProcentsInThisMonth = 0;
        }

        private decimal ForecastCurrencyRateOnFinish(Deposit deposit)
        {
            var currentRate = (decimal)_rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today);
            var delta = (currentRate - (decimal)_rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today.AddDays(-30))) / 30;
            return currentRate + (deposit.FinishDate - DateTime.Today).Days * delta;
        }

        private void CalculateUpToEndEstimatedProcents(Deposit deposit)
        {
            deposit.CalculationData.Estimations.PeriodForUpToEndPayment = new Period(deposit.GetDateOfLastProcentTransaction().AddDays(1), deposit.FinishDate);
            deposit.CalculationData.Estimations.ProcentsUpToFinish =
                deposit.CalculationData.DailyTable.Where(l => deposit.CalculationData.Estimations.PeriodForUpToEndPayment.ContainsButTimeNotChecking(l.Date)).Sum(l => l.DayProcents);

            if (deposit.DepositOffer.Currency != CurrencyCodes.USD)
                deposit.CalculationData.Estimations.CurrencyRateOnFinish = ForecastCurrencyRateOnFinish(deposit);
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
