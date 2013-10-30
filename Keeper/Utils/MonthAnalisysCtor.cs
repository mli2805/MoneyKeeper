using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class MonthAnalisysCtor
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }
  
    public static Saldo AnalizeMonth(DateTime initialDay)
    {
      var myAccountsRoot = (from account in Db.Accounts
                            where account.Name == "Мои"
                            select account).FirstOrDefault();
      var result = new Saldo();

      result.StartDate = initialDay.AddDays(-initialDay.Day + 1);
      result.BeginBalanceInCurrencies = Balance.AccountBalancePairsBeforeDay(myAccountsRoot, result.StartDate).ToList();
      result.BeginBalance = Balance.BalancePairsToUsd(result.BeginBalanceInCurrencies, result.StartDate.AddDays(-1));
      result.BeginByrRate = (decimal)Rate.GetRateThisDayOrBefore(CurrencyCodes.BYR, result.StartDate.AddDays(-1));

      var transactions = (from transaction in Db.Transactions
                     where (transaction.Operation == OperationType.Доход || 
                        transaction.Operation == OperationType.Расход ) && 
                       transaction.Timestamp.Month == result.StartDate.Month &&
                        transaction.Timestamp.Year == result.StartDate.Year
                     select transaction);
      foreach (var transaction in transactions)
      {
        var amountInUsd = Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
        if (transaction.Operation== OperationType.Доход)
          result.Incomes += amountInUsd;
        else
        {
          result.Expense += amountInUsd;
          if (amountInUsd >= 50) result.LargeExpense += amountInUsd;
        }
      }

      result.EndBalanceInCurrencies =
        Balance.AccountBalancePairsBeforeDay(myAccountsRoot, result.StartDate.AddMonths(1)).ToList();
      result.EndBalance = Balance.BalancePairsToUsd(result.EndBalanceInCurrencies,
                                                    result.StartDate.AddMonths(1).AddDays(-1));
      if (!transactions.Any())
      {
        result.LastDayWithTransactionsInMonth = result.StartDate;
        result.EndByrRate = result.BeginByrRate;
      }
      else
      {
        var lastTransaction = transactions.Last();
        result.LastDayWithTransactionsInMonth = lastTransaction.Timestamp.Date;
        result.EndByrRate = (decimal)Rate.GetRate(CurrencyCodes.BYR, lastTransaction.Timestamp);
      }

      return result;
    }

  }
}
