using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  class DumpDb
  {
    [Import]
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static void DumpAllTables()
    {
      if (!Directory.Exists(Settings.Default.DumpPath)) Directory.CreateDirectory(Settings.Default.DumpPath);
      DumpAccounts();
      DumpTransactions();
      DumpCurrencyRates();
      // TODO  DumpTransactions();
      MessageBox.Show("Выгрузка завершена успешно!", "Экспорт");
    }

    public static void DumpAccounts()
    {
      var content = new List<string>();
      var roots = new List<Account>(from account in Db.Accounts.Include("Children")
                                                        where account.Parent == null
                                                        select account);
      foreach (var accountsRoot in roots)
      {
        DumpAccount(accountsRoot, content);
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "Accounts.txt"), content);
    }

    public static void DumpAccount(Account account, List<string> content)
    {
      foreach (var child in account.Children)
      {
        DumpAccount(child, content);
      }
      content.Add(account.ToDump());
    }

  public static void DumpTransactions()
    {
      var content = new List<string>();
      foreach (var transaction in Db.Transactions)
      {
        content.Add(transaction.ToDump());
        File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "Transactions.txt"), content);
      }
    }

    public static void DumpCurrencyRates()
    {
      var content = new List<string>();
      foreach (var currencyRate in Db.CurrencyRates)
      {
        content.Add(currencyRate.ToDump());
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "CurrencyRates.txt"), content);
    }

  }
}
