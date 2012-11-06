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
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);

    public static void DumpAllTables()
    {
      if (!Directory.Exists(Settings.Default.DumpPath)) Directory.CreateDirectory(Settings.Default.DumpPath);
      DumpAccounts();
      DumpTransactions();
      DumpCurrencyRates();
      MessageBox.Show("Выгрузка завершена успешно!", "Экспорт");
    }

    #region // Accounts
    public static void DumpAccounts()
    {
      var content = new List<string>();
      var roots = new List<Account>(from account in Db.Accounts.Local
                                                        where account.Parent == null
                                                        select account);
      foreach (var accountsRoot in roots)
      {
        DumpAccount(accountsRoot, content, 0);
        content.Add("");
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "Accounts.txt"), content, Encoding1251);
    }

    public static void DumpAccount(Account account, List<string> content, int offset)
    {
      content.Add(account.ToDump(offset));
      foreach (var child in account.Children)
      {
        DumpAccount(child, content, offset+2);
      }
    }
    #endregion

    public static void DumpTransactions()
    {
      var content = new List<string>();
      foreach (var transaction in Db.Transactions)
      {
        content.Add(transaction.ToDumpWithNames());
        File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "Transactions.txt"), content, Encoding1251);
      }
    }

    public static void DumpCurrencyRates()
    {
      var content = new List<string>();
      foreach (var currencyRate in Db.CurrencyRates)
      {
        content.Add(currencyRate.ToDump());
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "CurrencyRates.txt"), content, Encoding1251);
    }

  }
}
