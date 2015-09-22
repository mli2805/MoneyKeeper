using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.ByFunctional.BalanceEvaluating
{
    [Export]
    public class ArticleBalanceCalculator
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public ArticleBalanceCalculator(KeeperDb db)
        {
            _db = db;
        }

        /// <summary>
        /// возвращает ОБОРОТ по СТАТЬЕ (доходов / расходов) переведенный в доллары и дополнительно
        /// список субсчетов если есть или список операций по счету
        /// </summary>
        /// <param name="article">статья (доходов / расходов)</param>
        /// <param name="period">временной интервал</param>
        /// <param name="transactions">список</param>
        /// <returns></returns>
        public decimal GetArticleSaldoInUsdPlusTransactions(Account article, Period period, List<string> transactions)
        {
            var transactionsWithRates = (from t in _db.Transactions
                                         where t.Article != null && t.Article.Is(article.Name) && period.ContainsAndTimeWasChecked(t.Timestamp)
                                         join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                                         from rate in g.DefaultIfEmpty()
                                         select new
                                         {
                                             t.Timestamp,
                                             AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                                             t.Comment
                                         }).ToList();

            var am = transactionsWithRates.Sum(t => t.AmountInUsd);

            if (am != 0 && article.Children.Count == 0)
            {
                transactions.Clear();
                for (var i = 0; i < transactionsWithRates.Count(); i++)
                {
                    transactions.Add(string.Format("  {0:dd/MM/yyyy} ${1:#,0} {2}", transactionsWithRates[i].Timestamp, transactionsWithRates[i].AmountInUsd, transactionsWithRates[i].Comment.Trim()));
                }
            }

            return am;
        }

        public decimal GetAccountSaldoInUsdPlusTransactions(Account account, Period period, List<string> transactions)
        {
            var transactionsWithRates = (from t in _db.Transactions
                                         where period.ContainsAndTimeWasChecked(t.Timestamp) && (t.Debet.Is(account.Name) || t.Credit.Is(account.Name))
                                         join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                                         from rate in g.DefaultIfEmpty()
                                         select new
                                         {
                                             t.Timestamp,
                                             AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate * t.SignForAmount(account) : t.Amount * t.SignForAmount(account),
                                             t.Comment
                                         }).ToList();

            var am = transactionsWithRates.Sum(t => t.AmountInUsd);

            if (am != 0 && account.Children.Count == 0)
            {
                transactions.Clear();
                for (var i = 0; i < transactionsWithRates.Count(); i++)
                {
                    transactions.Add(string.Format("  {0:dd/MM/yyyy} ${1:#,0} {2}", transactionsWithRates[i].Timestamp, transactionsWithRates[i].AmountInUsd, transactionsWithRates[i].Comment.Trim()));
                }
            }

            return am;
        }
    }
}