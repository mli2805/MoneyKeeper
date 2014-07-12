using System;
using System.Composition;
using System.Linq;
using System.Windows;
using Keeper.DomainModel;
using Keeper.Utils.Rates;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositAnalyser
    {
        private readonly RateExtractor _rateExtractor;
        private readonly DepositProcentsCalculator _depositProcentsCalculator;

        [ImportingConstructor]
        public DepositAnalyser(RateExtractor rateExtractor, DepositProcentsCalculator depositProcentsCalculator)
        {
            _rateExtractor = rateExtractor;
            _depositProcentsCalculator = depositProcentsCalculator;
        }

        public void MakeForecast(Account account)
        {
            var lastProcentTransaction = account.Deposit.Evaluations.Traffic.LastOrDefault(t => t.TransactionType == DepositOperations.Проценты);
            var lastProcentDate = lastProcentTransaction == null ? account.Deposit.StartDate : lastProcentTransaction.Timestamp;

            CalculateProcentsForPeriod(account, lastProcentDate);
            account.Deposit.Evaluations.EstimatedProfitInUsd = account.Deposit.Evaluations.CurrentProfit + _rateExtractor.GetUsdEquivalent(account.Deposit.Evaluations.EstimatedProcents, account.Deposit.Currency, DateTime.Today);
        }

        public void CalculateProcentsForPeriod(Account account, DateTime lastProcentDate)
        {
            if (account.Deposit.DepositRateLines != null)
                _depositProcentsCalculator.ProcentsForPeriod(account, new Period(lastProcentDate, account.Deposit.FinishDate));
            else
                MessageBox.Show("Не заведена таблица процентных ставок!");
        }

        public decimal GetProfitForYear(Deposit deposit, int year)
        {
            if (deposit.Evaluations.CurrentProfit == 0) return 0;
            var startYear = deposit.Evaluations.Traffic.First().Timestamp.Year;
            var finishYear = deposit.Evaluations.Traffic.Last().Timestamp.AddDays(-1).Year;
            if (year < startYear || year > finishYear) return 0;
            if (startYear == finishYear) return deposit.Evaluations.CurrentProfit;
            var allDaysCount = (deposit.Evaluations.Traffic.Last().Timestamp.AddDays(-1) - deposit.Evaluations.Traffic.First().Timestamp).Days;
            if (year == startYear)
            {
                var startYearDaysCount = (new DateTime(startYear, 12, 31) - deposit.Evaluations.Traffic.First().Timestamp).Days;
                return deposit.Evaluations.CurrentProfit * startYearDaysCount / allDaysCount;
            }
            if (year == finishYear)
            {
                var finishYearDaysCount = (deposit.Evaluations.Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
                return deposit.Evaluations.CurrentProfit * finishYearDaysCount / allDaysCount;
            }
            var yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
            return deposit.Evaluations.CurrentProfit * yearDaysCount / allDaysCount;
        }

    }
}