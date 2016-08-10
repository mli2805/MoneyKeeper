using System;
using System.Collections.Generic;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.BalanceEvaluating.Ilya;
using Keeper.Utils.DiagramDataExtraction;

namespace Keeper.DomainModel.WorkTypes
{
    public class MoneyBagWithTotal
    {
        public MoneyBag MoneyBag { get; set; }
        public decimal TotalInUsd { get; set; }
    }

    public class ExtendedBalance
    {
        public MoneyBagWithTotal Common { get; set; }
        public MoneyBagWithTotal OnHands { get; set; }
        public MoneyBagWithTotal OnDeposits { get; set; }

        public ExtendedBalance()
        {
            Common = new MoneyBagWithTotal();
            OnHands = new MoneyBagWithTotal();
            OnDeposits = new MoneyBagWithTotal();
        }
    }
    public class ExtendedTraffic
    {
        public List<TranForAnalysis> Trans { get; set; }
        public decimal TotalInUsd { get; set; }

        public ExtendedTraffic()
        {
            Trans = new List<TranForAnalysis>();
        }
    }

    public class ExtendedIncomes
    {
        public ExtendedTraffic OnDeposits { get; set; }
        public ExtendedTraffic OnHands { get; set; }
        public decimal TotalInUsd { get { return OnDeposits.TotalInUsd + OnHands.TotalInUsd; } }

        public ExtendedIncomes()
        {
            OnHands = new ExtendedTraffic();
            OnDeposits = new ExtendedTraffic();
        }
    }

    public class TranForAnalysis
    {
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public decimal AmountInUsd { get; set; }
        public Account Category { get; set; }
        public string Comment { get; set; }
        public bool IsDepositTran { get; set; }
        public string DepoName { get; set; }
    }

    public class ExtendedExpense
    {
        public List<TranForAnalysis> LargeTransactions { get; set; }
        public List<CategoriesDataElement> Categories { get; set; }
        public decimal TotalForLargeInUsd { get; set; }
        public decimal TotalInUsd { get; set; }

        public ExtendedExpense()
        {
            LargeTransactions = new List<TranForAnalysis>();
            Categories = new List<CategoriesDataElement>();
        }
    }

    public class EstimatedMoney
    {
        public decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public string ArticleName { get; set; }
    }

    public class EstimatedPayments
    {
        public List<EstimatedMoney> Payments { get; set; }
        public decimal EstimatedSum { get; set; }
        public decimal TotalInUsd { get; set; }

        public EstimatedPayments()
        {
            Payments = new List<EstimatedMoney>();
        }
    }

    public class DepoTraffic
    {
        public decimal ToDepo { get; set; }
        public decimal FromDepo { get; set; }
    }

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
