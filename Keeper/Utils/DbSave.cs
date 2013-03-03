using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  class DbSave
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);

    public static TimeSpan SaveAllTables()
    {
      var start = DateTime.Now;

      if (!Directory.Exists(Settings.Default.SavePath)) Directory.CreateDirectory(Settings.Default.SavePath);
      SaveAccounts();
      SaveTransactions();
      SaveArticlesAssociations();
      SaveCurrencyRates();

      ZipAllTables();

      return DateTime.Now - start;
    }

    public static void ZipAllTables()
    {
      var archiveName = String.Format("DB{0:yyyy-MM-dd-HH-mm-ss}.zip", DateTime.Now);
      var zipFileToCreate = Path.Combine(Settings.Default.KeeperInDropBox, archiveName);
      var directoryToZip = Settings.Default.SavePath;
      try
      {
        using (var zip = new ZipFile())
        {
          zip.Password = "!opa1526";
          zip.Encryption = EncryptionAlgorithm.WinZipAes256;
          var filenames = Directory.GetFiles(directoryToZip); // note: this does not recurse directories! 
          foreach (var filename in filenames)
            zip.AddFile(filename, String.Empty);
          zip.Comment = String.Format("This zip archive was created  on machine '{0}'", System.Net.Dns.GetHostName());
          zip.Save(zipFileToCreate);
        }
      }
      catch (Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    #region // Accounts
    public static void SaveAccounts()
    {
      var content = new List<string>();
      foreach (var accountsRoot in Db.Accounts)
      {
        SaveAccount(accountsRoot, content, 0);
        content.Add("");
      }
      File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "Accounts.txt"), content, Encoding1251);
    }

    public static void SaveAccount(Account account, List<string> content, int offset)
    {
      content.Add(account.ToDump(offset));
      foreach (var child in account.Children)
      {
        SaveAccount(child, content, offset + 2);
      }
    }
    #endregion

    public static void SaveTransactions()
    {
      var content = new List<string>();

      var orderedTransactions = from transaction in Db.Transactions
                                orderby transaction.Timestamp
                                select transaction;

      var prevTimestamp = new DateTime(2001, 1, 1);
      foreach (var transaction in orderedTransactions)
      {
        if (transaction.Timestamp <= prevTimestamp) transaction.Timestamp = prevTimestamp.AddMinutes(1);
        content.Add(transaction.ToDumpWithNames());
        prevTimestamp = transaction.Timestamp;
      }
      File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "Transactions.txt"), content, Encoding1251);
    }

    public static void SaveArticlesAssociations()
    {
      var content = Db.ArticlesAssociations.Select(association => association.ToDumpWithNames()).ToList();
      File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "ArticlesAssociations.txt"), content, Encoding1251);
    }

    public static void SaveCurrencyRates()
    {
      var ratesOrderedByDate = (from rate in Db.CurrencyRates
                               orderby rate.BankDay
                               select rate.ToDump()).ToList();

      File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "CurrencyRates.txt"), ratesOrderedByDate, Encoding1251);
    }

  }
}
