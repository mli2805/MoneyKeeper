using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput
{
  internal class DbToTxtSaver
  {
    public Encoding Encoding1251 = Encoding.GetEncoding(1251);

    public void MakeDbBackupCopy(KeeperDb db)
    {
      SaveDbInTxt(db);
      ZipTxtDb(db);
      DeleteTxtDb(db);
    }

    public void SaveDbInTxt(KeeperDb db)
    {
      if (!Directory.Exists(Settings.Default.TemporaryTxtDbPath))
        Directory.CreateDirectory(Settings.Default.TemporaryTxtDbPath);
      SaveAccounts(db);
      SaveTransactions(db);
      SaveArticlesAssociations(db);
      SaveCurrencyRates(db);
    }


    public void DeleteTxtDb(KeeperDb db)
    {
      if (!Directory.Exists(Settings.Default.TemporaryTxtDbPath)) return;
      var filenames = Directory.GetFiles(Settings.Default.TemporaryTxtDbPath, "*.txt"); // note: this does not recurse directories! 
      foreach (var filename in filenames)
      {
        File.Delete(filename);
      }
    }

    public void ZipTxtDb(KeeperDb db)
    {
      var archiveName = String.Format("DB{0:yyyy-MM-dd-HH-mm-ss}.zip", DateTime.Now);
      var zipFileToCreate = Path.Combine(Settings.Default.DbPath, archiveName);
      var directoryToZip = Settings.Default.TemporaryTxtDbPath;
      try
      {
        using (var zip = new ZipFile())
        {
          zip.Password = "!opa1526";
          zip.Encryption = EncryptionAlgorithm.WinZipAes256;
          var filenames = Directory.GetFiles(directoryToZip, "*.txt"); // note: this does not recurse directories! 
          foreach (var filename in filenames)
            zip.AddFile(filename, String.Empty);
          zip.Comment = String.Format("This zip archive was created  on machine '{0}'", System.Net.Dns.GetHostName());
          zip.Save(zipFileToCreate);
        }
      }
      catch (Exception ex1)
      {
        MessageBox.Show("Exception during database ziping: " + ex1);
      }
    }

    #region // Accounts

    private void SaveAccounts(KeeperDb db)
    {
      var content = new List<string>();
      foreach (var accountsRoot in db.Accounts)
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

    private void SaveTransactions(KeeperDb db)
    {
      var content = new List<string>();

      var orderedTransactions = from transaction in db.Transactions
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

    private void SaveArticlesAssociations(KeeperDb db)
    {
      var content = db.ArticlesAssociations.Select(association => association.ToDumpWithNames()).ToList();
      File.WriteAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, "ArticlesAssociations.txt"), content, Encoding1251);
    }

    private void SaveCurrencyRates(KeeperDb db)
    {
      var ratesOrderedByDate = (from rate in db.CurrencyRates
                                orderby rate.BankDay
                                select rate.ToDump()).ToList();

      File.WriteAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, "CurrencyRates.txt"), ratesOrderedByDate, Encoding1251);
    }

  }
}
