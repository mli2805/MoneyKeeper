using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ByFunctional.BalanceEvaluating.Ilya
{
  public static class TransactionExtensions
  {
    public static Money Debit(this Transaction transaction)
    {
      return new Money(transaction.Currency, transaction.Amount);
    }
    public static Money Credit(this Transaction transaction)
    {
        return new Money(transaction.Currency, transaction.Amount);
    }

    public static MoneyBag Credit(this IEnumerable<Transaction> transactions, Func<Transaction, bool> predicate = null)
    {
      predicate = predicate ?? (tr => true);
      return transactions.Where(predicate).Sum(t => t.Credit());
    }
    public static MoneyBag Debit(this IEnumerable<Transaction> transactions, Func<Transaction, bool> predicate = null)
    {
      predicate = predicate ?? (tr => true);
      return transactions.Where(predicate).Sum(t => t.Debit());
    }

    public static MoneyBag Balance(this IEnumerable<Transaction> transactions, Func<Transaction, bool> predicate = null)
    {
      predicate = predicate ?? (tr => true);
      return transactions.Where(predicate).Sum(t => t.Credit() - t.Debit());
    }

    public static MoneyBag Balance(this IEnumerable<Transaction> transactions, Account balancedAccount, Period interval)
    {
      return transactions.Balance(t => interval.ContainsAndTimeWasChecked(t.Timestamp) && t.EitherDebitOrCreditIs(balancedAccount));
    }

    public static bool DebitOrCreditIs(this Transaction transaction, Account account)
    {
      return transaction.Credit.Is(account) || transaction.Debet.Is(account);
    }
    public static bool EitherDebitOrCreditIs(this Transaction transaction, Account account)
    {
      return transaction.Credit.Is(account) != transaction.Debet.Is(account);
    }
  }
}