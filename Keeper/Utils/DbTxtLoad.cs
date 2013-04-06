using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  public class DbLoadError
  {
    public int Code { get; set; }
    public string Explanation { get; set; }

    public void Add(int code, string explanation)
    {
      Code = code;
      Explanation += explanation + "\n";
    }

  }
  class DbTxtLoad
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);
    public static DbLoadError Result = new DbLoadError();

    public static DbLoadError LoadDbFromTxt()
    {
      var tt = new Stopwatch();
      tt.Start();

      Db.Accounts = LoadAccounts();
      Db.AccountsPlaneList = KeeperDb.FillInAccountsPlaneList(Db.Accounts);

      Db.Transactions = LoadFrom("Transactions.txt", TransactionFromStringWithNames);
      Db.ArticlesAssociations = LoadFrom("ArticlesAssociations.txt", ArticleAssociationFromStringWithNames);
      Db.CurrencyRates = LoadFrom("CurrencyRates.txt", CurrencyRateFromString);

      tt.Stop();
      Console.WriteLine("Creation backup copy in encrypted zip archive takes {0} sec", tt.Elapsed);

      return Result;
    }

    private static string GetLatestDbArchive()
    {
      return "Db.zip";
    }

    public static void UnzipAllTables()
    {
      var zipToUnpack = GetLatestDbArchive();
      var unpackDirectory = Settings.Default.SavePath;
      using (var zip1 = ZipFile.Read(zipToUnpack))
      {
        // here, we extract every entry, but we could extract conditionally
        // based on entry name, size, date, checkbox status, etc.  
        foreach (ZipEntry e in zip1)
        {
          e.ExtractWithPassword(unpackDirectory, ExtractExistingFileAction.OverwriteSilently, "!opa1526");
        }
      }
    }

    public static ObservableCollection<T> LoadFrom<T>(string filename, Func<string, T> parseLine)
    {
      var content =
        File.ReadAllLines(Path.Combine(Settings.Default.SavePath, filename), Encoding1251).Where(
          s => !string.IsNullOrWhiteSpace(s));
      var wrongContent = new List<string>();
      var result = new ObservableCollection<T>();

      foreach (var s in content)
      {
        try
        {
          result.Add(parseLine(s));
        }
        catch (Exception)
        {
          wrongContent.Add(s);
        }
      }
      if (wrongContent.Count != 0)
      {
        File.WriteAllLines(Path.ChangeExtension(Path.Combine(Settings.Default.SavePath, filename), "err"), wrongContent, Encoding1251);
        Result.Add(6, "Ошибки загрузки смотри в файле " + Path.ChangeExtension(filename, "err"));
      }
      return result;
    }

    #region // Accounts
    public static ObservableCollection<Account> LoadAccounts()
    {
      var content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "Accounts.txt"), Encoding1251).
                                                                 Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
      var result = new ObservableCollection<Account>();
      foreach (var s in content)
      {
        int parentId;
        var account = AccountFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, content);
          result.Add(account);
        }
      }
      return result;
    }

    private static Account AccountFromString(string s, out int parentId)
    {
      var account = new Account();
      var substrings = s.Split(';');
      account.Id = Convert.ToInt32(substrings[0]);
      account.Name = substrings[1].Trim();
      parentId = Convert.ToInt32(substrings[2]);
      account.IsExpanded = Convert.ToBoolean(substrings[3]);
      return account;
    }

    private static void BuildBranchFromRoot(Account root, List<string> content)
    {
      foreach (var s in content)
      {
        if (s == "") continue;
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

    #endregion

    #region // Parsing
    private static Transaction TransactionFromStringWithNames(string s)
    {
      var transaction = new Transaction();
      var substrings = s.Split(';');
      transaction.Timestamp = Convert.ToDateTime(substrings[0]);
      transaction.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
      transaction.Debet = Db.AccountsPlaneList.First(account => account.Name == substrings[2].Trim());
      transaction.Credit = Db.AccountsPlaneList.First(account => account.Name == substrings[3].Trim());
      transaction.Amount = Convert.ToDecimal(substrings[4]);
      transaction.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[5]);
      transaction.Amount2 = Convert.ToDecimal(substrings[6]);
      if (substrings[7].Trim() == "null" || substrings[7].Trim() == "0") transaction.Currency2 = null;
      else
        transaction.Currency2 = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[7]);
      transaction.Article = substrings[8].Trim() != "" ? Db.AccountsPlaneList.First(account => account.Name == substrings[8].Trim()) : null;
      transaction.Comment = substrings[9].Trim();

      return transaction;
    }
    private static CurrencyRate CurrencyRateFromString(string s)
    {
      var rate = new CurrencyRate();
      int next = s.IndexOf(';');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, next));
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(next + 2, 3));
      next += 6;
      rate.Rate = Convert.ToDouble(s.Substring(next + 2));
      return rate;
    }
    private static ArticleAssociation ArticleAssociationFromStringWithNames(string s)
    {
      var association = new ArticleAssociation();
      var substrings = s.Split(';');
      association.ExternalAccount = Db.AccountsPlaneList.First(account => account.Name == substrings[0].Trim());
      association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
      association.AssociatedArticle = Db.AccountsPlaneList.First(account => account.Name == substrings[2].Trim());
      return association;
    }
    #endregion
  }
}
