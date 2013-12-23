using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [Export]
  public class DbToTxtSaver : IDbToTxtSaver
  {
    private readonly KeeperDb _db;
    public Encoding Encoding1251 = Encoding.GetEncoding(1251);

    [ImportingConstructor]
    public DbToTxtSaver(KeeperDb db)
    {
      _db = db;
    }

    public void SaveDbInTxt()
    {
      if (!Directory.Exists(Settings.Default.TemporaryTxtDbPath))
        Directory.CreateDirectory(Settings.Default.TemporaryTxtDbPath);
      SaveAccounts();
      SaveTransactions();
      SaveArticlesAssociations();
      SaveCurrencyRates();
    }

    #region // Accounts

    private void SaveAccounts()
    {
      var content = new List<string>();
      foreach (var accountsRoot in _db.Accounts)
      {
        SaveAccount(accountsRoot, content, 0);
        content.Add("");
      }
      File.WriteAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, "Accounts.txt"), content, Encoding1251);
    }

    private void SaveAccount(Account account, List<string> content, int offset)
    {
      content.Add(account.ToDump(offset));
      foreach (var child in account.Children)
      {
        SaveAccount(child, content, offset + 2);
      }
    }
    #endregion

    private void SaveTransactions()
    {
      var content = new List<string>();

      var orderedTransactions = from transaction in _db.Transactions
                                orderby transaction.Timestamp
                                select transaction;

      var prevTimestamp = new DateTime(2001, 1, 1);
      foreach (var transaction in orderedTransactions)
      {
        if (transaction.Timestamp <= prevTimestamp) transaction.Timestamp = prevTimestamp.AddMinutes(1);
        content.Add(transaction.ToDumpWithNames());
        prevTimestamp = transaction.Timestamp;
      }
      File.WriteAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, "Transactions.txt"), content, Encoding1251);
    }

    private void SaveArticlesAssociations()
    {
      var content = _db.ArticlesAssociations.Select(association => association.ToDumpWithNames()).ToList();
      File.WriteAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, "ArticlesAssociations.txt"), content, Encoding1251);
    }

    private void SaveCurrencyRates()
    {
      var ratesOrderedByDate = (from rate in _db.CurrencyRates
                                orderby rate.BankDay
                                select rate.ToDump()).ToList();

      File.WriteAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, "CurrencyRates.txt"), ratesOrderedByDate, Encoding1251);
    }

  }
}
