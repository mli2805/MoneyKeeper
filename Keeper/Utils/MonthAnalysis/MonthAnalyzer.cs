using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.Common;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
    [Export]
    class MonthAnalyzer
    {
        public Saldo Result { get; set; }

        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;
        private readonly TransactionSetConvertor _transactionSetConvertor;
        private readonly MonthForecaster _monthForecaster;
        private readonly MySettings _mySettings;
        private readonly BalanceForMonthAnalysisCalculator _balanceCalculator;
        private readonly AccountTreeStraightener _accountTreeStraightener;

        [ImportingConstructor]
        public MonthAnalyzer(KeeperDb db, BalanceForMonthAnalysisCalculator balanceCalculator, MySettings mySettings,
                             AccountTreeStraightener accountTreeStraightener, RateExtractor rateExtractor,
                             TransactionSetConvertor transactionSetConvertor, MonthForecaster monthForecaster)
        {
            _db = db;
            _balanceCalculator = balanceCalculator;
            _accountTreeStraightener = accountTreeStraightener;
            _rateExtractor = rateExtractor;
            _transactionSetConvertor = transactionSetConvertor;
            _monthForecaster = monthForecaster;
            _mySettings = mySettings;

            Result = new Saldo();
        }

        private IEnumerable<Transaction> GetMonthTransactionsForAnalysis(OperationType operationType,
                                              DateTime someDate, IEnumerable<Transaction> transactions)
        {
            return (from transaction in transactions
                    where transaction.Guid == Guid.Empty && transaction.Operation == operationType &&
                    transaction.Timestamp.Month == someDate.Month && transaction.Timestamp.Year == someDate.Year
                    select transaction);
        }

        private IEnumerable<Transaction> GetMonthTransactionsForAnalysis(
                                              DateTime someDate, IEnumerable<Transaction> transactions)
        {
            return (from transaction in transactions
                    where transaction.Timestamp.Month == someDate.Month && transaction.Timestamp.Year == someDate.Year
                    select transaction);
        }

        private void RegisterIncome(Transaction transaction)
        {
            var amountInUsd = _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
            if (transaction.Credit.Deposit != null || transaction.Credit.Is("Депозиты"))
            {
                Result.Incomes.OnDeposits.Transactions.Add(transaction);
                Result.Incomes.OnDeposits.TotalInUsd += amountInUsd;
            }
            else
            {
                Result.Incomes.OnHands.Transactions.Add(transaction);
                Result.Incomes.OnHands.TotalInUsd += amountInUsd;
            }
        }

        private void RegisterExpense(IEnumerable<Transaction> expenseTransactions)
        {
            var expenseTransactionsInUsd = _transactionSetConvertor.ConvertTransactionsQuery(expenseTransactions);
            GroupExpenseByCategories(expenseTransactionsInUsd);
            Result.Expense.TotalInUsd = expenseTransactionsInUsd.Sum(t => t.AmountInUsd);

            var largeTransactions = expenseTransactionsInUsd.Where(transaction => Math.Abs(transaction.AmountInUsd) > (decimal)_mySettings.GetSetting("LargeExpenseUsd"));
            foreach (var transaction in largeTransactions)
            {
                Result.Expense.LargeTransactions.Add(transaction);
            }
            Result.Expense.TotalForLargeInUsd = largeTransactions.Sum(t => t.AmountInUsd);
        }

        private void GroupExpenseByCategories(IEnumerable<ConvertedTransaction> expenseTransactionsInUsd)
        {
            var expenseRoot = _accountTreeStraightener.Seek("Все расходы", _db.Accounts);
            foreach (var expense in expenseRoot.Children)
            {
                var amountInUsd = (from e in expenseTransactionsInUsd
                                   where e.Article.Is(expense.Name)
                                   select e).Sum(a => a.AmountInUsd);

                if (amountInUsd != 0)
                    Result.Expense.Categories.Add(new BalanceTrio { Amount = amountInUsd, Currency = CurrencyCodes.USD, MyAccount = expense });
            }
        }


        private void RegisterDepositsTraffic(List<Transaction> allMonthTransactions)
        {
            Result.TransferFromDeposit = allMonthTransactions.
              Where(t => t.Debet.IsDeposit() && !t.Credit.IsDeposit()).
              Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp));

            Result.TransferToDeposit = allMonthTransactions.
              Where(t => !t.Debet.IsDeposit() && t.Credit.IsDeposit()).
              Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp)) - Result.Incomes.OnDeposits.TotalInUsd;
        }

        private List<CurrencyRate> InitializeRates(DateTime date)
        {
            var result = new List<CurrencyRate>();
            var currencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>();
            foreach (CurrencyCodes currencyCode in currencyList)
            {
                if (currencyCode != CurrencyCodes.USD) result.Add(_rateExtractor.FindRateForDateOrBefore(currencyCode, date));
            }
            return result;
        }

        public Saldo AnalizeMonth(DateTime initialDay)
        {
            var sw = new Stopwatch();
            sw.Start();

            Result = new Saldo();
            Result.StartDate = initialDay.AddDays(-initialDay.Day + 1);
            Result.BeginBalance = _balanceCalculator.GetExtendedBalanceBeforeDate(Result.StartDate);
            Result.BeginRates = InitializeRates(Result.StartDate.AddDays(-1));

            var incomeTransactions = GetMonthTransactionsForAnalysis(OperationType.Доход, Result.StartDate, _db.Transactions);
            foreach (var transaction in incomeTransactions) RegisterIncome(transaction);

            RegisterExpense(GetMonthTransactionsForAnalysis(OperationType.Расход, Result.StartDate, _db.Transactions));

            RegisterDepositsTraffic(GetMonthTransactionsForAnalysis(Result.StartDate, _db.Transactions).ToList());

            Result.EndBalance = _balanceCalculator.GetExtendedBalanceBeforeDate(Result.StartDate.AddMonths(1));

            Result.EndRates = InitializeRates(Result.StartDate.AddMonths(1));

            _monthForecaster.CollectEstimates(Result);

            sw.Stop();
            Console.WriteLine("Month analysis takes {0}", sw.Elapsed);

            return Result;
        }

    }
}
