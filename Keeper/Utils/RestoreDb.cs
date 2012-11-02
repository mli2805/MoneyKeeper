using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  class RestoreDb
  {
    [Import]
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static void RestoreAllTables()
    {
      RestoreCurrencyRates();
      RestoreAccounts();
      RestoreTransactions();
    }

    private static void RestoreTransactions()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "Transactions.txt"));
      foreach (var s in content)
      {
        var transaction = TransactionFromString(s);
        Db.Transactions.Add(transaction);
      }
    }

    private static Transaction TransactionFromString(string s)
    {
      var transaction = new Transaction();
      int prev = s.IndexOf(';');
      transaction.Timestamp = Convert.ToDateTime(s.Substring(0, prev));
      int next = s.IndexOf(';', prev + 2);
      transaction.Operation = (OperationType)Enum.Parse(typeof (OperationType), s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      int debetId = Convert.ToInt32(s.Substring(prev + 2, next - prev - 3));
      transaction.Debet = Db.Accounts.Local.First(account => account.Id == debetId);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      int creditId = Convert.ToInt32(s.Substring(prev + 2, next - prev - 3));
      transaction.Credit = Db.Accounts.Local.First(account => account.Id == creditId);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      int articleId = Convert.ToInt32(s.Substring(prev + 2, next - prev - 3));
      transaction.Article = Db.Accounts.Local.First(account => account.Id == articleId);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Amount = Convert.ToDecimal(s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Currency = (CurrencyCodes) Enum.Parse(typeof (CurrencyCodes), s.Substring(prev + 2, next - prev - 3));
      transaction.Comment = s.Substring(next + 2);

      return transaction;
    }

    public static void RestoreCurrencyRates()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "CurrencyRates.txt"));
      foreach (var s in content)
      {
        var rate = CurrencyRateFromString(s);
        Db.CurrencyRates.Add(rate);
      }
    }

    private static CurrencyRate CurrencyRateFromString(string s)
    {
      var rate = new CurrencyRate();
      int next = s.IndexOf(';');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, next));
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(next + 2, 3));
      next += 6;
      rate.Rate = Convert.ToDouble(s.Substring(next+2));
      return rate;
    }

    public static void RestoreAccounts()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "Accounts.txt"));
      foreach (var s in content)
      {
        int parentId;
        var account = AccountFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, content);
          Db.Accounts.Add(account);
        }
      }
    }

    private static Account AccountFromString(string s, out int parentId)
    {
      var account = new Account();
      int prev = s.IndexOf(';');
      account.Id = Convert.ToInt32(s.Substring(0, prev));
      int next = s.IndexOf(';', prev + 2);
      account.Name = s.Substring(prev + 2, next - prev - 3);
      parentId = Convert.ToInt32(s.Substring(next + 2));
      return account;
    }

    private static void BuildBranchFromRoot(Account root, string[] content)
    {
      foreach (var s in content)
      {
        int parentId;
        var account = AccountFromString(s, out parentId);
        if (parentId == root.Id)
        {
          account.Parent = root;
          root.Children.Add(account);
          BuildBranchFromRoot(account, content);
        }
      }
    }

 }
}
