using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Keeper.Utils.Balances
{
  public static class TransactionExtensions
  {
    public static Money Debit(this Transaction transaction)
    {
      return new Money(transaction.Currency, transaction.Amount);
    }
    public static Money Credit(this Transaction transaction)
    {
      return transaction.Operation != OperationType.Ξαμεν
               ? new Money(transaction.Currency, transaction.Amount)
               : new Money(transaction.Currency2.GetValueOrDefault(), transaction.Amount2);
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
      return transactions.Balance(t => interval.Contains(t.Timestamp) && t.EitherDebitOrCreditIs(balancedAccount));
    }

    public static bool DebitOrCreditIs(this Transaction transaction, Account account)
    {
      return transaction.Credit.Is(account) || transaction.Debet.Is(account);
    }
    public static bool EitherDebitOrCreditIs(this Transaction transaction, Account account)
    {
      var damper = IoC.Get<DbClassesInstanceDumper>();
//      return transaction.Credit.Is(account) != transaction.Debet.Is(account);
      if (transaction.Credit.Is(account) != transaction.Debet.Is(account))
      {
        if (account.Id == 424)
        {
          if (transaction.Credit.Is(account))
               Console.WriteLine(damper.Dump(transaction));
        }
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}