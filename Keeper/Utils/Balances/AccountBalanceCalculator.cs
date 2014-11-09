using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Utils.Balances
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

    private IEnumerable<MoneyPair> GetAccountBalancePairs(Account balancedAccount, Period interval)
    {
      var transactions = from t in _db.Transactions
                         where interval.ContainsAndTimeWasChecked(t.Timestamp) && t.EitherDebitOrCreditIs(balancedAccount)
                         select t;
             
      IEnumerable<MoneyPair> moneyPairs = from t in transactions
                                          group t by t.Currency
                                          into g
                                          select new MoneyPair{ Currency = g.Key, Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount)) };
      
      // учесть вторую сторону обмена - приход денег в другой валюте
      IEnumerable<MoneyPair> moneyPairsExchange =
        from t in _db.Transactions
        where t.Amount2 != 0 && interval.ContainsAndTimeWasChecked(t.Timestamp) && (t.Credit.Is(balancedAccount.Name) || t.Debet.Is(balancedAccount.Name))
        group t by t.Currency2 into g
        select new MoneyPair { Currency = (CurrencyCodes)g.Key, Amount = g.Sum(a => a.Amount2 * a.SignForAmount(balancedAccount) * -1) };

      var tempBalance = moneyPairs.Concat(moneyPairsExchange);

      IEnumerable<MoneyPair> accountBalancePairs = from b in tempBalance group b by b.Currency into g select new MoneyPair { Currency = g.Key, Amount = g.Sum(a => a.Amount) };
      return accountBalancePairs;
    }

    public IEnumerable<MoneyPair> GetAccountBalancePairsFromMidnightToMidnight(Account balancedAccount, Period interval)
    {
      var intervalUpToMidnight = new Period(interval.Start.GetStartOfDate(),interval.Finish.GetEndOfDate());
      return GetAccountBalancePairs(balancedAccount, intervalUpToMidnight);
    }

    public IEnumerable<MoneyPair> GetAccountBalancePairsWithTimeChecking(Account balancedAccount, Period interval)
    {
      return GetAccountBalancePairs(balancedAccount, interval);
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
      var balances = GetAccountBalancePairsWithTimeChecking(account, period);
      foreach (var balancePair in balances)
      {
        if (balancePair.Currency == currency) return balancePair.Amount;
      }
      return 0;
    }
  }
}
