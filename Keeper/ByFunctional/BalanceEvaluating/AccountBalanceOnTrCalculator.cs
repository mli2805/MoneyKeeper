using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;

namespace Keeper.ByFunctional.BalanceEvaluating
{
    public class AccountBalanceOnTrCalculator
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]

        public AccountBalanceOnTrCalculator(KeeperDb db)
        {
            _db = db;
        }

        public IEnumerable<MoneyPair> GetAccountBalancePairs(Account balancedAccount, Period interval)
        {
            List<TrBase> transactions = (from t in _db.Trs
                                              where interval.ContainsAndTimeWasChecked(t.Timestamp) && t.MyAccount.Is(balancedAccount)
                                              select t).ToList();

            List<MoneyPair> moneyPairs = (from t in transactions
                                          group t by t.Currency
                                              into g
                                          select new MoneyPair { Currency = g.Key, Amount = (decimal)g.Sum(a => a.Amount * a.SignForAmount(balancedAccount)) }).ToList();




            IEnumerable<MoneyPair> accountBalancePairs = from b in moneyPairs group b by b.Currency into g select new MoneyPair { Currency = g.Key, Amount = g.Sum(a => a.Amount) };
            return accountBalancePairs;
        }

    }
}
