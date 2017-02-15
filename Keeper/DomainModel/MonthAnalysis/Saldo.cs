using System;
using System.Collections.Generic;
using Keeper.DomainModel.DbTypes;

namespace Keeper.DomainModel.MonthAnalysis
{
    public class Saldo
    {
        public DateTime StartDate { get; set; }
        public ExtendedBalance BeginBalance { get; set; }
        public List<CurrencyRate> BeginRates { get; set; }
        public ExtendedIncomes Incomes { get; set; }
        public ExtendedExpense Expense { get; set; }

        public decimal ExchangeDifference { get { return EndBalance.Common.TotalInUsd - BeginBalance.Common.TotalInUsd - Incomes.TotalInUsd + Expense.TotalInUsd; } }
        public decimal ExchangeDepositDifference
        {
            get
            { return EndBalance.OnDeposits.TotalInUsd - BeginBalance.OnDeposits.TotalInUsd - Incomes.OnDeposits.TotalInUsd - DepoTraffic.ToDepo + DepoTraffic.FromDepo; }
        }

        public ExtendedBalance EndBalance { get; set; }
        public List<CurrencyRate> EndRates { get; set; }

        public DepoTraffic DepoTraffic { get; set; }
        public EstimatedPayments ForecastRegularIncome { get; set; }
        public EstimatedPayments ForecastRegularExpense { get; set; }
        public decimal ForecastExpense { get; set; }
        public decimal ForecastFinResult { get { return ForecastRegularIncome.TotalInUsd - ForecastExpense; } }
        public decimal ForecastEndBalance { get { return BeginBalance.Common.TotalInUsd + ForecastFinResult; } }

        public Saldo()
        {
            Incomes = new ExtendedIncomes();
            Expense = new ExtendedExpense();
            ForecastRegularExpense = new EstimatedPayments();
        }
    }

}
