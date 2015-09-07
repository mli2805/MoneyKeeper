using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Common;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class DepositCalculationAggregator
    {
        private readonly DepositCalculator _depositCalculator;

        [ImportingConstructor]
        public DepositCalculationAggregator(DepositCalculator depositCalculator)
        {
            _depositCalculator = depositCalculator;
        }

        public void FillinFieldsForOneDepositReport(Deposit deposit)
        {
            _depositCalculator.Calculate(deposit);
            if (deposit.CalculationData.State == DepositStates.Закрыт || deposit.CalculationData.State == DepositStates.Просрочен) return;
            CalculateMonthEstimatedProcents(deposit, DateTime.Today);
            CalculateUpToEndEstimatedProcents(deposit);
            ForecastResult(deposit);
        }

        public void FillinFieldsForMonthAnalysis(Deposit deposit, DateTime day)
        {
            _depositCalculator.Calculate(deposit);
            if (day.IsMonthTheSame(DateTime.Today)) CalculateMonthEstimatedProcents(deposit, day);
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
            }
            else deposit.CalculationData.Estimations.ProcentsInThisMonth = 0;
        }

        private void CalculateUpToEndEstimatedProcents(Deposit deposit)
        {
            deposit.CalculationData.Estimations.PeriodForUpToEndPayment = new Period(deposit.GetDateOfLastProcentTransaction().AddDays(1), deposit.FinishDate);
            deposit.CalculationData.Estimations.ProcentsUpToFinish =
                deposit.CalculationData.DailyTable.Where(l => deposit.CalculationData.Estimations.PeriodForUpToEndPayment.ContainsButTimeNotChecking(l.Date)).Sum(l => l.DayProcents);
        }

        private void ForecastResult(Deposit deposit)
        {
            if (deposit.DepositOffer.Currency != CurrencyCodes.USD)
            {
                var todayLine = deposit.CalculationData.DailyTable.First(l => l.Date.Date == DateTime.Today.Date);
                var finishLine = deposit.CalculationData.DailyTable.Last();
                deposit.CalculationData.Estimations.DevaluationInUsd = finishLine.Balance/finishLine.CurrencyRate -
                                                                       todayLine.Balance/todayLine.CurrencyRate;

                deposit.CalculationData.Estimations.ProfitInUsd =
                    deposit.CalculationData.TotalPercentInUsd
                    + deposit.CalculationData.Estimations.ProcentsUpToFinish / deposit.CalculationData.DailyTable.Last().CurrencyRate
                    + deposit.CalculationData.CurrentDevaluationInUsd
                    + deposit.CalculationData.Estimations.DevaluationInUsd;
            }
            else
                deposit.CalculationData.Estimations.ProfitInUsd =
                    deposit.CalculationData.TotalPercentInUsd + deposit.CalculationData.Estimations.ProcentsUpToFinish;

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
