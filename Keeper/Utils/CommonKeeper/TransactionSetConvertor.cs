using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class TransactionSetConvertor
    {
        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public TransactionSetConvertor(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        /// <summary>
        /// работает гораздо быстрее чем через foreach,
        /// но если нет курса за день операции, 
        /// не может взять предшествующий курс
        /// </summary>
        /// <param name="someSetOfTransactions"></param>
        /// <returns></returns>
        public IEnumerable<ConvertedTransaction> ConvertTransactionsQuery(IEnumerable<Transaction> someSetOfTransactions)
        {
            return from t in someSetOfTransactions
                   join r in _db.CurrencyRates
                     on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new ConvertedTransaction
                   {
                       Timestamp = t.Timestamp,
                       Amount = t.Amount,
                       Currency = t.Currency,
                       Article = t.Article,
                       AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                       Comment = t.Comment
                   };
        }

        public IEnumerable<ClassifiedTran> GetClassifiedExpenses()
        {
            return from t in _db.TransWithTags
                   where t.Operation == OperationType.Расход
                   join r in _db.CurrencyRates
                     on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new ClassifiedTran
                   {
                       Timestamp = t.Timestamp,
                       Category = GetExpenseTranCategory(t),
                       AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                   };
        }

        private Account GetExpenseTranCategory(TranWithTags tran)
        {
            var categories = _db.Accounts.First(a => a.Name == "Все расходы").Children.ToList();
            return tran.Tags.Intersect(categories).FirstOrDefault();
        }

    }
}
