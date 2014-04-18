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
using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [Export(typeof(IDbFromTxtLoader))]
  [Export(typeof(ILoader))]
  public class DbFromTxtLoader : IDbFromTxtLoader, ILoader
	{
    private readonly DbClassesInstanceParser _dbClassesInstanceParser;
    public Encoding Encoding1251 = Encoding.GetEncoding(1251);
	  public DbLoadResult Result;

    public string FileExtension { get { return ".txt"; } }

    public DbLoadResult Load(string filename)
    {
      return LoadDbFromTxt(Path.GetDirectoryName(filename));
    }

	  [ImportingConstructor]
	  public DbFromTxtLoader(DbClassesInstanceParser dbClassesInstanceParser)
	  {
	    _dbClassesInstanceParser = dbClassesInstanceParser;
	  }

    public DbLoadResult LoadDbFromTxt(string path)
    {
      var db = new KeeperDb();
      db.Accounts = LoadAccounts(path);
      if (Result != null) return Result;

      db.Transactions = LoadFrom(path, "Transactions.txt", _dbClassesInstanceParser.TransactionFromStringWithNames, new AccountTreeStraightener().Flatten(db.Accounts));
      if (Result != null) return Result;
      db.ArticlesAssociations = LoadFrom(path, "ArticlesAssociations.txt", _dbClassesInstanceParser.ArticleAssociationFromStringWithNames, new AccountTreeStraightener().Flatten(db.Accounts));
      if (Result != null) return Result;
      db.CurrencyRates = LoadFrom(path, "CurrencyRates.txt", _dbClassesInstanceParser.CurrencyRateFromString, new AccountTreeStraightener().Flatten(db.Accounts));
      if (Result != null) return Result;

      return new DbLoadResult(db);
    }

    private ObservableCollection<T> LoadFrom<T>(string path, string filename, Func<string, IEnumerable<Account>, T> parseLine, IEnumerable<Account> accountsPlaneList)
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

      var accounts = new ObservableCollection<Account>();
      foreach (var s in content)
      {
        int parentId;
        var account = _dbClassesInstanceParser.AccountFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, content);
          accounts.Add(account);
        }
      }
      return accounts;
    }

    private void BuildBranchFromRoot(Account root, List<string> content)
    {
      foreach (var s in content)
      {
        if (s == "") continue;
        int parentId;
        var account = _dbClassesInstanceParser.AccountFromString(s, out parentId);
        if (parentId == root.Id)
        {
          account.Parent = root;
          root.Children.Add(account);
          BuildBranchFromRoot(account, content);
        }
      }
    }

    #endregion


	}
}
