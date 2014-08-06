using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Common;

namespace Keeper.Utils.Deposits
{
    /// <summary>
    /// Type001 для депозитов с фиксированной процентной ставкой
    /// и начислением процентов в день открытия вклада
    /// 
    /// Например: ВТБ Бонус
    /// </summary>
    [Export]
    public class BankDepositCalculatorType001 : IBankDepositCalculator
    {
        private readonly DepositProcentsCalculator _depositProcentsCalculator;

        [ImportingConstructor]
        public BankDepositCalculatorType001(DepositProcentsCalculator depositProcentsCalculator)
        {
            _depositProcentsCalculator = depositProcentsCalculator;
        }

        public decimal GetThisMonthEstimatedProcents(Deposit deposit)
        {
            var lastProcentTransaction = deposit.CalculationData.Traffic.LastOrDefault(t => t.TransactionType == DepositTransactionTypes.Проценты);
            var lastProcentDate = lastProcentTransaction == null ? deposit.StartDate : lastProcentTransaction.Timestamp;
            if (lastProcentDate.IsMonthTheSame(DateTime.Today))
            {
                deposit.CalculationData.EstimatedProcentsInThisMonth = 0;
                return 0;
            }

            _depositProcentsCalculator.FillinProcents(deposit.ParentAccount); // вот здесь считаются проценты за каждый день
            var periodWithoutProcent = new Period(lastProcentDate.AddDays(1),
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, lastProcentDate.Day));

            deposit.CalculationData.EstimatedProcentsInThisMonth =
                deposit.CalculationData.DailyTable.Where(l=>periodWithoutProcent.Contains(l.Date)).Sum(l => l.DayProfit);
            return deposit.CalculationData.EstimatedProcentsInThisMonth;
        }

        public decimal GetUpToEndEstimatedProcents(Deposit deposit)
        {
            deposit.CalculationData.EstimatedProcents = 10;
            return 10;
        }

        public decimal GetEstimatedProcentsForPeriod(Deposit deposit, Period period)
        {
            return 7;
        }
    }
}
