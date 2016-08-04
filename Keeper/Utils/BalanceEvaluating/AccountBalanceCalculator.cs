using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
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
