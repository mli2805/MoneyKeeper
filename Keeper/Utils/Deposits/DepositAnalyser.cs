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
            var lastProcentTransaction = account.Deposit.CalculatedTotals.Traffic.LastOrDefault(t => t.TransactionType == DepositTransactionTypes.Проценты);
            var lastProcentDate = lastProcentTransaction == null ? account.Deposit.StartDate : lastProcentTransaction.Timestamp;

            CalculateProcentsForPeriod(account, lastProcentDate);
            account.Deposit.CalculatedTotals.EstimatedProfitInUsd = account.Deposit.CalculatedTotals.CurrentProfit + _rateExtractor.GetUsdEquivalent(account.Deposit.CalculatedTotals.EstimatedProcents, account.Deposit.Currency, DateTime.Today);
        }

        public void CalculateProcentsForPeriod(Account account, DateTime lastProcentDate)
        {
            if (account.Deposit.RateLines != null)
                _depositProcentsCalculator.ProcentsForPeriod(account, new Period(lastProcentDate, account.Deposit.FinishDate));
            else
                MessageBox.Show("Не заведена таблица процентных ставок!");
        }

        public decimal GetProfitForYear(Deposit deposit, int year)
        {
            if (deposit.CalculatedTotals.CurrentProfit == 0) return 0;
            var startYear = deposit.CalculatedTotals.Traffic.First().Timestamp.Year;
            var finishYear = deposit.CalculatedTotals.Traffic.Last().Timestamp.AddDays(-1).Year;
            if (year < startYear || year > finishYear) return 0;
            if (startYear == finishYear) return deposit.CalculatedTotals.CurrentProfit;
            var allDaysCount = (deposit.CalculatedTotals.Traffic.Last().Timestamp.AddDays(-1) - deposit.CalculatedTotals.Traffic.First().Timestamp).Days;
            if (year == startYear)
            {
                var startYearDaysCount = (new DateTime(startYear, 12, 31) - deposit.CalculatedTotals.Traffic.First().Timestamp).Days;
                return deposit.CalculatedTotals.CurrentProfit * startYearDaysCount / allDaysCount;
            }
            if (year == finishYear)
            {
                var finishYearDaysCount = (deposit.CalculatedTotals.Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
                return deposit.CalculatedTotals.CurrentProfit * finishYearDaysCount / allDaysCount;
            }
            var yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
            return deposit.CalculatedTotals.CurrentProfit * yearDaysCount / allDaysCount;
        }

    }
}