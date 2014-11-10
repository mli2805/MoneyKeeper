using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.BalanceEvaluating
{
  public enum TransactionTypeForAnalysis
  {
    ЛюбаяМоя,
    Депозитная,
    МояНоНеДепозитная
  }

  [Export]
  public class BalanceForMonthAnalysisCalculator
  {
    private readonly KeeperDb _db;
    private readonly ICurrencyConverter _currencyConverter;

    [ImportingConstructor]
    public BalanceForMonthAnalysisCalculator(KeeperDb db, ICurrencyConverter currencyConverter)
    {
      _db = db;
      _currencyConverter = currencyConverter;
    }

    private IEnumerable<MoneyPair> MonthAnalisysBalancePairs(DateTime startDay, TransactionTypeForAnalysis flag)
    {
      var transactions = from t in _db.Transactions
                         where t.Timestamp.Date < startDay.Date && t.IsAcceptableForMonthAnalysisAs(flag)
                         select t;

      IEnumerable<MoneyPair> firstPart = from t in transactions
                                         group t by t.Currency
                                         into g
                                         select new MoneyPair
                                                  {
                                                    Currency = g.Key,
                                                    Amount = g.Sum(t => t.Amount * t.SignForAnalysis(flag))
                                                  };

      // учесть вторую сторону обмена - приход денег в другой валюте
      var exchangeTransactions = from t in _db.Transactions
                                 where t.Timestamp.Date < startDay.Date &&
                                       t.Amount2 != 0 && t.IsAcceptableForMonthAnalysisAs(flag)
                                 select t;


      IEnumerable<MoneyPair> secondPart = from t in exchangeTransactions
                                          group t by t.Currency2 into g
                                          select new MoneyPair
                                                   {
                                                     Currency = (CurrencyCodes)g.Key,
                                                     Amount = g.Sum(t => t.Amount2 * -t.SignForAnalysis(flag))
                                                   };

      var tempBalance = firstPart.Concat(secondPart);

      IEnumerable<MoneyPair> accountBalancePairs = from b in tempBalance
                                                   group b by b.Currency into g
                                                   select new MoneyPair { Currency = g.Key, Amount = g.Sum(a => a.Amount) };
      return accountBalancePairs;
    }

    private ExtendedBalance GetExtendedBalanceBeforeDate(DateTime startDay, TransactionTypeForAnalysis flag)
    {
      var extendedBalance = new ExtendedBalance {InCurrencies = MonthAnalisysBalancePairs(startDay, flag).ToList()};
      extendedBalance.TotalInUsd = _currencyConverter.BalancePairsToUsd(extendedBalance.InCurrencies, startDay.AddDays(-1));
      return extendedBalance;
    }
    
    public ExtendedBalanceForAnalysis GetExtendedBalanceBeforeDate(DateTime startDay)
    {
      return new ExtendedBalanceForAnalysis
               {
                 Common = GetExtendedBalanceBeforeDate(startDay, TransactionTypeForAnalysis.ЛюбаяМоя),
                 OnHands = GetExtendedBalanceBeforeDate(startDay, TransactionTypeForAnalysis.МояНоНеДепозитная),
                 OnDeposits = GetExtendedBalanceBeforeDate(startDay, TransactionTypeForAnalysis.Депозитная)
               };
    }
  }
}