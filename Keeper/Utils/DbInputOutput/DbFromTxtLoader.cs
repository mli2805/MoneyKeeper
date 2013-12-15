using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.DbInputOutput
{
	[Export(typeof(IDbFromTxtLoader))]
  class DbFromTxtLoader : IDbFromTxtLoader
	{

    public Encoding Encoding1251 = Encoding.GetEncoding(1251);
    public DbLoadError Result = new DbLoadError();

    public DbLoadError LoadFromLastZip(ref KeeperDb db)
    {
      return LoadDbFromTxt(ref db);
    }

    public DbLoadError LoadDbFromTxt(ref KeeperDb db)
    {
      LoadAccounts(ref db);
      if (db.Accounts == null)
      {
        Result.Code = 5;
        Result.Explanation = "File '" + Path.Combine(Settings.Default.TemporaryTxtDbPath, "Accounts.txt") + "' not found";
        return Result;
      }
      db.AccountsPlaneList = KeeperDb.FillInAccountsPlaneList(db.Accounts);

      db.Transactions = LoadFrom("Transactions.txt", TransactionFromStringWithNames, db.AccountsPlaneList);
      db.ArticlesAssociations = LoadFrom("ArticlesAssociations.txt", ArticleAssociationFromStringWithNames, db.AccountsPlaneList);
      db.CurrencyRates = LoadFrom("CurrencyRates.txt", CurrencyRateFromString, db.AccountsPlaneList);

      return Result;
    }

    private string GetLatestDbArchive()
    {
      return "Db.zip";
    }

    public void UnzipAllTables()
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

    public ObservableCollection<T> LoadFrom<T>(string filename, Func<string, List<Account>, T> parseLine, List<Account> accountsPlaneList)
    {
      var content =
        File.ReadAllLines(Path.Combine(Settings.Default.TemporaryTxtDbPath, filename), Encoding1251).Where(
          s => !string.IsNullOrWhiteSpace(s));
      var wrongContent = new List<string>();
      var result = new ObservableCollection<T>();

      foreach (var s in content)
      {
        try
        {
          result.Add(parseLine(s, accountsPlaneList));
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
    public bool LoadAccounts(ref KeeperDb db)
    {
      var filename = Path.Combine(Settings.Default.TemporaryTxtDbPath, "Accounts.txt");
      if (!File.Exists(filename)) return false;

      var content = File.ReadAllLines(filename, Encoding1251).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

      // промежуточная сортировка действующих депозитов
      var sortedContent = SortActiveDepositAccountsByEndDate(content);

      db.Accounts = new ObservableCollection<Account>();
      foreach (var s in sortedContent)
      {
        int parentId;
        var account = AccountFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, sortedContent);
          db.Accounts.Add(account);
        }
      }
      return true;
    }

    private List<string> SortActiveDepositAccountsByEndDate(IEnumerable<string> content)
    {
      var activeDepos = new List<Account>();
      var sortedContent = new List<string>();
      int depoId = -1;
      var depoAccount = new Account();
      foreach (var s in content)
      {
        int parentId;
        var account = AccountFromString(s, out parentId);
        if (account.Name == "Депозиты")
        {
          depoId = account.Id;
          depoAccount = account;
        }
        if (parentId == depoId && account.Name != "Закрытые депозиты")
        {
          account.Parent = depoAccount;
          activeDepos.Add(account);
        }
        else sortedContent.Add(s);
      }

      activeDepos.Sort(Account.CompareEndDepositDates);

      sortedContent.AddRange(activeDepos.Select(account => account.ToDump(4)));
      return sortedContent;
    }

    private Account AccountFromString(string s, out int parentId)
    {
      var account = new Account();
      var substrings = s.Split(';');
      account.Id = Convert.ToInt32(substrings[0]);
      account.Name = substrings[1].Trim();
      parentId = Convert.ToInt32(substrings[2]);
      account.IsExpanded = Convert.ToBoolean(substrings[3]);
      return account;
    }

    private void BuildBranchFromRoot(Account root, List<string> content)
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
    private Transaction TransactionFromStringWithNames(string s, List<Account> accountsPlaneList)
    {
      var transaction = new Transaction();
      var substrings = s.Split(';');
      transaction.Timestamp = Convert.ToDateTime(substrings[0]);
      transaction.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
      transaction.Debet = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
      transaction.Credit = accountsPlaneList.First(account => account.Name == substrings[3].Trim());
      transaction.Amount = Convert.ToDecimal(substrings[4]);
      transaction.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[5]);
      transaction.Amount2 = Convert.ToDecimal(substrings[6]);
      if (substrings[7].Trim() == "null" || substrings[7].Trim() == "0") transaction.Currency2 = null;
      else
        transaction.Currency2 = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[7]);
      transaction.Article = substrings[8].Trim() != "" ? accountsPlaneList.First(account => account.Name == substrings[8].Trim()) : null;
      transaction.Comment = substrings[9].Trim();

      return transaction;
    }
    private CurrencyRate CurrencyRateFromString(string s, List<Account> accountsPlaneList)
    {
      var rate = new CurrencyRate();
      int next = s.IndexOf(';');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, next));
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(next + 2, 3));
      next += 6;
      rate.Rate = Convert.ToDouble(s.Substring(next + 2));
      return rate;
    }
    private ArticleAssociation ArticleAssociationFromStringWithNames(string s, List<Account> accountsPlaneList)
    {
      var association = new ArticleAssociation();
      var substrings = s.Split(';');
      association.ExternalAccount = accountsPlaneList.First(account => account.Name == substrings[0].Trim());
      association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
      association.AssociatedArticle = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
      return association;
    }
    #endregion
  }
}
