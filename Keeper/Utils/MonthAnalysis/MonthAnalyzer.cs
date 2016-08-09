using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.Common;
using Keeper.Utils.DiagramDataExtraction;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
    [Export]
    class MonthAnalyzer
    {
        public Saldo Result { get; set; }

        private readonly KeeperDb _db;
        private readonly MySettings _mySettings;
        private readonly RateExtractor _rateExtractor;
        private readonly MonthForecaster _monthForecaster;
        private readonly BalanceForMonthAnalysisCalculator _balanceCalculator;

        [ImportingConstructor]
        public MonthAnalyzer(KeeperDb db, MySettings mySettings,
            BalanceForMonthAnalysisCalculator balanceCalculator,
                             RateExtractor rateExtractor,
                             MonthForecaster monthForecaster
                             )
        {
            _db = db;
            _mySettings = mySettings;
            _balanceCalculator = balanceCalculator;
            _rateExtractor = rateExtractor;
            _monthForecaster = monthForecaster;

            Result = new Saldo();
        }

        private IEnumerable<TranForAnalysis> GetTransForAnalysis(bool isIncome, DateTime someDate)
        {
            return from t in _db.TransWithTags
                   where t.Timestamp.IsMonthTheSame(someDate) && t.Operation == (isIncome ? OperationType.Доход : OperationType.Расход)
                   join r in _db.CurrencyRates on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals
                       new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new TranForAnalysis()
                   {
                       Timestamp = t.Timestamp,
                       Amount = t.Amount,
                       Currency = t.Currency.GetValueOrDefault(),
                       AmountInUsd = t.Currency == CurrencyCodes.USD ? t.Amount : t.Amount / (rate == null ? 1 : (decimal)rate.Rate),
                       Category = isIncome ? t.GetTranArticle(isIncome) : t.GetTranCategory(isIncome),
                       Comment = t.Comment,
                       IsDepositTran = t.MyAccount.IsDeposit(),
                       DepoName = t.MyAccount.IsDeposit() ? t.MyAccount.Deposit.ShortName : "",
                   };
        }
        private void RegisterAllIncome(DateTime someDate)
        {
            var trans = GetTransForAnalysis(true, someDate);

            Result.Incomes.OnDeposits.Trans.AddRange(trans.Where(t => t.IsDepositTran));
            Result.Incomes.OnDeposits.TotalInUsd = trans.Where(t => t.IsDepositTran).Sum(t => t.AmountInUsd);
            Result.Incomes.OnHands.Trans.AddRange(trans.Where(t => !t.IsDepositTran));
            Result.Incomes.OnHands.TotalInUsd = trans.Where(t => !t.IsDepositTran).Sum(t => t.AmountInUsd);
        }

        private void RegisterAllExpense(DateTime someDate)
        {
            var trans = GetTransForAnalysis(false, someDate);

            Result.Expense.LargeTransactions.AddRange(trans.Where(t=> Math.Abs(t.AmountInUsd) > (decimal)_mySettings.GetSetting("LargeExpenseUsd")));
            Result.Expense.TotalForLargeInUsd = Result.Expense.LargeTransactions.Sum(l => l.AmountInUsd);

            var categories = from t in trans
                group t by t.Category
                into g
                select new CategoriesDataElement(g.Key, g.Sum(a => a.AmountInUsd), new YearMonth(g.First().Timestamp.Year, g.First().Timestamp.Month));

            Result.Expense.Categories.AddRange(categories);
            Result.Expense.TotalInUsd = Result.Expense.Categories.Sum(l => l.Amount);
        }

        private void RegisterDepoTraffic(DateTime someDate)
        {
            var allTrans = from t in _db.TransWithTags where t.Timestamp.IsMonthTheSame(someDate) select t;

            Result.TransferFromDeposit =                          // расход (не может быть) или перенос со вклада
                allTrans.Where(t => (t.Operation == OperationType.Расход && t.MyAccount.IsDeposit()) || (t.Operation == OperationType.Перенос && t.MyAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency.GetValueOrDefault(), t.Timestamp)) +
                // перенос со вклада с обменом
                allTrans.Where(t => (t.Operation == OperationType.Обмен && t.MyAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency.GetValueOrDefault(), t.Timestamp));

            Result.TransferToDeposit =                           // доход (%%) или перенос во вклад
                allTrans.Where(t => (t.Operation == OperationType.Доход && t.MyAccount.IsDeposit()) || (t.Operation == OperationType.Перенос && t.MySecondAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency.GetValueOrDefault(), t.Timestamp)) +
                // перенос во вклад с обменом
                allTrans.Where(t => (t.Operation == OperationType.Обмен && t.MySecondAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.AmountInReturn, t.CurrencyInReturn.GetValueOrDefault(), t.Timestamp)) - Result.Incomes.OnDeposits.TotalInUsd;

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

            RegisterAllIncome(initialDay);
            RegisterAllExpense(initialDay);
            RegisterDepoTraffic(initialDay);

            Result.EndBalance = _balanceCalculator.GetExtendedBalanceBeforeDate(Result.StartDate.AddMonths(1));

            Result.EndRates = InitializeRates(Result.StartDate.AddMonths(1));

            _monthForecaster.CollectEstimates(Result);

            sw.Stop();
            Console.WriteLine("Month analysis takes {0}", sw.Elapsed);

            return Result;
        }

    }
}
