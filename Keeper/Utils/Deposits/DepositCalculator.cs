using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using System.Linq;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Common;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositCalculator
    {
        private readonly DepositExtractor _depositExtractor;

        private Deposit _deposit;

        [ImportingConstructor]
        public DepositCalculator(DepositExtractor depositExtractor)
        {
            _depositExtractor = depositExtractor;
        }

        public void Calculate(Deposit deposit)
        {
            _deposit = deposit;

            _depositExtractor.Extract(_deposit.ParentAccount);
            CalculateDailyProcents();
            CalculateThisMonthEstimatedProcents();
        }

        private void CalculateDailyProcents()
        {
            foreach (var day in _deposit.CalculationData.DailyTable)
            {
                day.DepoRate = GetCorrespondingDepoRate(day.Balance, day.Date);
                day.DayProfit = CalculateOneDayProcents(day.DepoRate, day.Balance);
            }
        }

        /// <summary>
        /// вот здесь реальные отличия в расчете процентов 
        /// выше по стеку идет суммирование расчитанных здесь процентов
        /// 
        /// </summary>
        /// <param name="balance"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private decimal GetCorrespondingDepoRate(decimal balance, DateTime date)
        {
            var line = _deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= balance && l.AmountTo >= balance && l.DateFrom < date);
            return line == null ? 0 : line.Rate;
        }

        private decimal CalculateOneDayProcents(decimal depoRate, decimal balance)
        {
            //      var year = deposit.IsFactDays ? 365 : 360;
            var year = _deposit.DepositOffer.CalculatingRules.IsFactDays ? 365 : 360;
            return balance * depoRate / 100 / year;
        }

        public void CalculateThisMonthEstimatedProcents()
        {
            var lastProcentTransaction = _deposit.CalculationData.Traffic.LastOrDefault(t => t.TransactionType == DepositTransactionTypes.Проценты);
            var lastProcentDate = lastProcentTransaction == null ? _deposit.StartDate : lastProcentTransaction.Timestamp;
            if (lastProcentDate.IsMonthTheSame(DateTime.Today))
            {
                _deposit.CalculationData.EstimatedProcentsInThisMonth = 0;
            }

            var periodWithoutProcent = new Period(lastProcentDate.AddDays(1),
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, lastProcentDate.Day));

            _deposit.CalculationData.EstimatedProcentsInThisMonth =
                _deposit.CalculationData.DailyTable.Where(l => periodWithoutProcent.Contains(l.Date)).Sum(l => l.DayProfit);
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
