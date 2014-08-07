using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Common;

namespace Keeper.Utils.Deposits
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
            CalculateThisMonthEstimatedProcents(deposit);
            CalculateUpToEndEstimatedProcents(deposit);
        }

        public void FillinFieldsForMonthAnalysis(Deposit deposit, DateTime day)
        {
            _depositCalculator.Calculate(deposit);
            CalculateMonthEstimatedProcents(deposit, day);
        }


        private void CalculateMonthEstimatedProcents(Deposit deposit, DateTime day)
        {
            var lastProcentDate = GetLastProcentDate(deposit);
            if (lastProcentDate.IsMonthTheSame(day))
            {
                deposit.CalculationData.EstimatedProcentsInThisMonth = 0;
            }

            var periodWithoutProcent = new Period(lastProcentDate.AddDays(1),
                new DateTime(day.Year, day.Month, lastProcentDate.Day));

            deposit.CalculationData.EstimatedProcentsInThisMonth =
                deposit.CalculationData.DailyTable.Where(l => periodWithoutProcent.Contains(l.Date)).Sum(l => l.DayProfit);

        }

        private void CalculateThisMonthEstimatedProcents(Deposit deposit)
        {
            CalculateMonthEstimatedProcents(deposit, DateTime.Today);
        }

        private static DateTime GetLastProcentDate(Deposit deposit)
        {
            var lastProcentTransaction =
                deposit.CalculationData.Traffic.LastOrDefault(t => t.TransactionType == DepositTransactionTypes.Проценты);
            return lastProcentTransaction == null ? deposit.StartDate : lastProcentTransaction.Timestamp;
        }

        private void CalculateUpToEndEstimatedProcents(Deposit deposit)
        {
            var lastProcentTransaction = deposit.CalculationData.Traffic.LastOrDefault(t => t.TransactionType == DepositTransactionTypes.Проценты);
            var lastProcentDate = lastProcentTransaction == null ? deposit.StartDate : lastProcentTransaction.Timestamp;

            var periodFromLastProcentToEnd = new Period(lastProcentDate.AddDays(1), deposit.FinishDate);

            deposit.CalculationData.EstimatedProcents =
                deposit.CalculationData.DailyTable.Where(l => periodFromLastProcentToEnd.Contains(l.Date)).Sum(l => l.DayProfit);
        }

        public decimal GetProfitForYear(Deposit deposit, int year)
        {
            if (deposit.CalculationData.CurrentProfit == 0) return 0;
            var startYear = deposit.CalculationData.Traffic.First().Timestamp.Year;
            var finishYear = deposit.CalculationData.Traffic.Last().Timestamp.AddDays(-1).Year;
            if (year < startYear || year > finishYear) return 0;
            if (startYear == finishYear) return deposit.CalculationData.CurrentProfit;
            var allDaysCount = (deposit.CalculationData.Traffic.Last().Timestamp.AddDays(-1) - deposit.CalculationData.Traffic.First().Timestamp).Days;
            if (year == startYear)
            {
                var startYearDaysCount = (new DateTime(startYear, 12, 31) - deposit.CalculationData.Traffic.First().Timestamp).Days;
                return deposit.CalculationData.CurrentProfit * startYearDaysCount / allDaysCount;
            }
            if (year == finishYear)
            {
                var finishYearDaysCount = (deposit.CalculationData.Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
                return deposit.CalculationData.CurrentProfit * finishYearDaysCount / allDaysCount;
            }
            var yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
            return deposit.CalculationData.CurrentProfit * yearDaysCount / allDaysCount;
        }

    }
}
