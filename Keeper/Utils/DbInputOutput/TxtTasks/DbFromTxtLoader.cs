using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.Utils.DbInputOutput
{
  [Export(typeof(IDbFromTxtLoader))]
  [Export(typeof(ILoader))]
  class DbFromTxtLoader : IDbFromTxtLoader, ILoader
	{
    public Encoding Encoding1251 = Encoding.GetEncoding(1251);
	  public DbLoadResult Result;

    public string FileExtension { get { return ".txt"; } }

    public DbLoadResult Load(string filename)
    {
      return LoadDbFromTxt(Path.GetDirectoryName(filename));
    }

    public DbLoadResult LoadDbFromTxt(string path)
    {
      var db = new KeeperDb();
      db.Accounts = LoadAccounts(path);
      if (Result != null) return Result; 
      db.AccountsPlaneList = new AccountTreeStraightener().FillInAccountsPlaneList(db.Accounts);

      db.Transactions = LoadFrom(path,"Transactions.txt", TransactionFromStringWithNames, db.AccountsPlaneList);
      if (Result != null) return Result;
      db.ArticlesAssociations = LoadFrom(path, "ArticlesAssociations.txt", ArticleAssociationFromStringWithNames, db.AccountsPlaneList);
      if (Result != null) return Result;
      db.CurrencyRates = LoadFrom(path, "CurrencyRates.txt", CurrencyRateFromString, db.AccountsPlaneList);
      if (Result != null) return Result;

      return new DbLoadResult(db);
    }

    private ObservableCollection<T> LoadFrom<T>(string path, string filename, Func<string, List<Account>, T> parseLine, List<Account> accountsPlaneList)
    {
      var fullFilename = Path.Combine(path, filename);
      if (!File.Exists(fullFilename))
      {
        Result = new DbLoadResult(325, "File <"+fullFilename+"> not found");
        return null;
      }

      var content = File.ReadAllLines(fullFilename, Encoding1251).Where(s => !String.IsNullOrWhiteSpace(s));
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
        File.WriteAllLines(Path.ChangeExtension(Path.Combine(path, filename), "err"), wrongContent, Encoding1251);
        Result = new DbLoadResult(326, "Ошибки загрузки смотри в файле " + Path.ChangeExtension(filename, "err"));
      }
      return result;
    }

    #region // Accounts
    private ObservableCollection<Account> LoadAccounts(string path)
    {
      var filename = Path.Combine(path, "Accounts.txt");
      if (!File.Exists(filename))
      {
        Result = new DbLoadResult(315, "File <Accounts.txt> not found");
        return null;
      }

      var content = File.ReadAllLines(filename, Encoding1251).Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

      // промежуточная сортировка действующих депозитов
      var sortedContent = SortActiveDepositAccountsByEndDate(content);

      var accounts = new ObservableCollection<Account>();
      foreach (var s in sortedContent)
      {
        int parentId;
        var account = AccountFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, sortedContent);
          accounts.Add(account);
        }
      }
      return accounts;
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
      transaction.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
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
      rate.BankDay = Convert.ToDateTime(s.Substring(0, next), new CultureInfo("ru-RU"));
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
