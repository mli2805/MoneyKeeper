using System;
using System.Composition;
using System.Linq;
using System.Windows;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositAnalyser
    {
        private readonly BankDepositCalculatorType001 _bankDepositCalculator;

        [ImportingConstructor]
        public DepositAnalyser(BankDepositCalculatorType001 bankDepositCalculator)
        {
            _bankDepositCalculator = bankDepositCalculator;
        }

        public void MakeForecast(Account account)
        {
            account.Deposit.CalculatedTotals.EstimatedProcentsInThisMonth = _bankDepositCalculator.GetThisMonthEstimatedProcents(account.Deposit);
        }

        public void CalculateProcentsForPeriod(Account account, DateTime lastProcentDate)
        {
            if (account.Deposit.DepositOffer.RateLines != null)
                _bankDepositCalculator.GetEstimatedProcentsForPeriod(account.Deposit, new Period(lastProcentDate, account.Deposit.FinishDate));
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