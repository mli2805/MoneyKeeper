using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating.Ilya;

namespace Keeper.Utils.BalanceEvaluating
{
    [Export]
    public class AccountBalanceCalculator
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public AccountBalanceCalculator(KeeperDb db)
        {
            _db = db;
        }

        /// <summary>
        /// учитывает не только даты в интервале, но и время!
        /// </summary>
        /// <param name="balancedAccount"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public IEnumerable<MoneyPair> GetAccountBalancePairs(Account balancedAccount, Period interval)
        {
            List<Transaction> transactions = (from t in _db.Transactions
                where interval.ContainsAndTimeWasChecked(t.Timestamp) && t.EitherDebitOrCreditIs(balancedAccount)
                select t).ToList();

            List<MoneyPair> moneyPairs = (from t in transactions
                group t by t.Currency
                into g
                select new MoneyPair {Currency = g.Key, Amount = g.Sum(a => a.Amount*a.SignForAmount(balancedAccount))})
                .ToList();

            IEnumerable<MoneyPair> accountBalancePairs = from b in moneyPairs
                group b by b.Currency
                into g
                select new MoneyPair {Currency = g.Key, Amount = g.Sum(a => a.Amount)};
            return accountBalancePairs;
        }

        /// <summary>
        /// возвращает ОБОРОТ по СЧЕТУ
        /// </summary>
        /// <param name="account"></param>
        /// <param name="period"></param>
        /// <param name="transactions"></param>
        /// <returns></returns>
        public decimal GetAccountSaldoInUsdPlusTransactions(Account account, Period period, List<string> transactions)
        {
            var transactionsWithRates = (from t in _db.Transactions
                where
                    period.ContainsAndTimeWasChecked(t.Timestamp) && !t.IsExchange() &&
                    (t.Debet.Is(account.Name) || t.Credit.Is(account.Name))
                join r in _db.CurrencyRates on new {t.Timestamp.Date, t.Currency} equals
                    new {r.BankDay.Date, r.Currency} into g
                from rate in g.DefaultIfEmpty()
                select new
                {
                    t.Timestamp,
                    AmountInUsd =
                        rate != null
                            ? t.Amount/(decimal) rate.Rate*t.SignForAmount(account)
                            : t.Amount*t.SignForAmount(account),
                    t.Comment
                }).ToList();

            var am = transactionsWithRates.Sum(t => t.AmountInUsd);

            if (am != 0 && account.Children.Count == 0)
            {
                transactions.Clear();
                for (var i = 0; i < transactionsWithRates.Count(); i++)
                {
                    transactions.Add(string.Format("  {0:dd/MM/yyyy} ${1:#,0} {2}", transactionsWithRates[i].Timestamp,
                        transactionsWithRates[i].AmountInUsd, transactionsWithRates[i].Comment.Trim()));
                }
            }

            return am;
        }


        /// <summary>
        /// Хреново!!! - запрашивает остаток по всем валютам, и возращает по одной переданной в качестве параметра 
        /// Иначе надо почти дублировать длинные GetAccountBalancePairs и GetArticleBalancePairs, только с параметром валюта
        /// Если будет где-то тормозить, можно переписать
        /// </summary>
        /// <param name="account">счет, по которому будет вычислен остаток</param>
        /// <param name="period">период, за который учитываются обороты</param>
        /// <param name="currency">валюта, в которой учитываются обороты</param>
        /// <returns></returns>
        public decimal GetAccountBalanceOnlyForCurrency(Account account, Period period, CurrencyCodes currency)
        {
            if (account == null) return 0;
            var balances = GetAccountBalancePairs(account, period);
            return
                (from balancePair in balances where balancePair.Currency == currency select balancePair.Amount)
                    .FirstOrDefault();
        }

    }
}
