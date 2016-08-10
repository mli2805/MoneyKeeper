using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Common;
using Keeper.Utils.DiagramDataExtraction;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
    [Export]
    class MonthTrafficAnalyzer
    {
        private readonly KeeperDb _db;
        private readonly MySettings _mySettings;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public MonthTrafficAnalyzer(KeeperDb db, MySettings mySettings, RateExtractor rateExtractor)
        {
            _db = db;
            _mySettings = mySettings;
            _rateExtractor = rateExtractor;
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
        public ExtendedIncomes GetIncomes(DateTime someDate)
        {
            var trans = GetTransForAnalysis(true, someDate);

            var result = new ExtendedIncomes();
            result.OnDeposits.Trans.AddRange(trans.Where(t => t.IsDepositTran));
            result.OnDeposits.TotalInUsd = trans.Where(t => t.IsDepositTran).Sum(t => t.AmountInUsd);
            result.OnHands.Trans.AddRange(trans.Where(t => !t.IsDepositTran));
            result.OnHands.TotalInUsd = trans.Where(t => !t.IsDepositTran).Sum(t => t.AmountInUsd);
            return result;
        }

        public ExtendedExpense GetExpense(DateTime someDate)
        {
            var trans = GetTransForAnalysis(false, someDate);

            var result = new ExtendedExpense();
            result.LargeTransactions.AddRange(trans.Where(t => Math.Abs(t.AmountInUsd) > (decimal)_mySettings.GetSetting("LargeExpenseUsd")));
            result.TotalForLargeInUsd = result.LargeTransactions.Sum(l => l.AmountInUsd);

            var categories = from t in trans
                group t by t.Category
                into g
                select new CategoriesDataElement(g.Key, g.Sum(a => a.AmountInUsd), new YearMonth(g.First().Timestamp.Year, g.First().Timestamp.Month));

            result.Categories.AddRange(categories);
            result.TotalInUsd = result.Categories.Sum(l => l.Amount);
            return result;
        }

        public DepoTraffic GetDepoTraffic(DateTime someDate, decimal incomesOnDeposit)
        {
            var result = new DepoTraffic();
            var allTrans = from t in _db.TransWithTags where t.Timestamp.IsMonthTheSame(someDate) select t;

            result.FromDepo =                          // расход (не может быть) или перенос со вклада
                allTrans.Where(t => (t.Operation == OperationType.Расход && t.MyAccount.IsDeposit()) || (t.Operation == OperationType.Перенос && t.MyAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency.GetValueOrDefault(), t.Timestamp)) +
                // перенос со вклада с обменом
                allTrans.Where(t => (t.Operation == OperationType.Обмен && t.MyAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency.GetValueOrDefault(), t.Timestamp));

            result.ToDepo = // доход (%%) или перенос во вклад
                allTrans.Where(t => (t.Operation == OperationType.Доход && t.MyAccount.IsDeposit()) ||
                                    (t.Operation == OperationType.Перенос && t.MySecondAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency.GetValueOrDefault(), t.Timestamp)) +
                // перенос во вклад с обменом
                allTrans.Where(t => (t.Operation == OperationType.Обмен && t.MySecondAccount.IsDeposit()))
                    .Sum(t => _rateExtractor.GetUsdEquivalent(t.AmountInReturn, t.CurrencyInReturn.GetValueOrDefault(), 
                        t.Timestamp)) - incomesOnDeposit;

            return result;
        }
    }
}