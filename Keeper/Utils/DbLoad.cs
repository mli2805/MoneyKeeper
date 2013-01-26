using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  class DbLoad
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);

    public static TimeSpan LoadAllTables()
    {
      var start = DateTime.Now;

      LoadCurrencyRates();
      LoadAccounts();
      FillInAccountsPlaneList(Db.Accounts);
      LoadTransactions();
      LoadArticlesAssociations();

      return DateTime.Now - start;
    }

    public static void FillInAccountsPlaneList(IEnumerable<Account> accountsList)
    {
      if (Db.AccountsPlaneList == null) Db.AccountsPlaneList = new List<Account>();
      foreach (var account in accountsList)
      {
        Db.AccountsPlaneList.Add(account);
        FillInAccountsPlaneList(account.Children);
      }
    }

    #region // Articles associations
    private static void LoadArticlesAssociations()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "ArticlesAssociations.txt"), Encoding1251);
      if (Db.ArticlesAssociations == null) Db.ArticlesAssociations = new ObservableCollection<ArticleAssociation>();
      var wrongContent = new List<string>();
      foreach (var s in content)
      {
        if (s == "") continue;

        ArticleAssociation association = null;
        try
        {
          association = ArticleAssociationFromStringWithNames(s);
        }
        catch (Exception)
        {
          wrongContent.Add(s);
        }
        if (association != null) Db.ArticlesAssociations.Add(association);
      }
      if (wrongContent.Count !=0) File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "LoadArticlesAssociations.err"), wrongContent, Encoding1251);
    }

    private static ArticleAssociation ArticleAssociationFromStringWithNames(string s)
    {
      var association = new ArticleAssociation();
      int prev = s.IndexOf(';');
      string externalAccount = s.Substring(0, prev - 1);
      association.ExternalAccount = Db.AccountsPlaneList.First(account => account.Name == externalAccount);
      int next = s.IndexOf(';', prev + 2);
      association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), s.Substring(prev + 2, next - prev - 3));
      string associatedArticle = s.Substring(next + 2);
      association.AssociatedArticle = Db.AccountsPlaneList.First(account => account.Name == associatedArticle);

      return association;
    }
    #endregion

    #region // Transactions
    private static void LoadTransactions()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "Transactions.txt"), Encoding1251);
      if (Db.Transactions == null) Db.Transactions = new ObservableCollection<Transaction>();
      var wrongContent = new List<string>();
      foreach (var s in content)
      {
        if (s == "") continue;

        Transaction transaction = null;
        try
        {
          transaction = TransactionFromStringWithNames(s);
        }
        catch (Exception)
        {
          wrongContent.Add(s);
        }
        if (transaction != null) Db.Transactions.Add(transaction);
      }
      if (wrongContent.Count !=0) File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "LoadTransactions.err"), wrongContent, Encoding1251);
    }

    private static Transaction TransactionFromStringWithNames(string s)
    {
      var transaction = new Transaction();
      int prev = s.IndexOf(';');
      transaction.Timestamp = Convert.ToDateTime(s.Substring(0, prev));
      int next = s.IndexOf(';', prev + 2);
      transaction.Operation = (OperationType)Enum.Parse(typeof(OperationType), s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      string debet = s.Substring(prev + 2, next - prev - 3);
      transaction.Debet = Db.AccountsPlaneList.First(account => account.Name == debet);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      string credit = s.Substring(prev + 2, next - prev - 3);
      transaction.Credit = Db.AccountsPlaneList.First(account => account.Name == credit);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Amount = Convert.ToDecimal(s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Amount2 = Convert.ToDecimal(s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      var c2 = s.Substring(prev + 2, next - prev - 3);
      if (c2 == "null" || c2 == "0") transaction.Currency2 = null;
      else
        transaction.Currency2 = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), c2);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      string article = s.Substring(prev + 2, next - prev - 3);
      transaction.Article = article != "" ? Db.AccountsPlaneList.First(account => account.Name == article) : null;
      transaction.Comment = s.Substring(next + 2);

      return transaction;
    }
    #endregion

    #region // Currency Rates
    public static void LoadCurrencyRates()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "CurrencyRates.txt"), Encoding1251);
      if (Db.CurrencyRates == null) Db.CurrencyRates = new ObservableCollection<CurrencyRate>();
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
      rate.Rate = Convert.ToDouble(s.Substring(next + 2));
      return rate;
    }
    #endregion

    #region // Accounts
    public static void LoadAccounts()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "Accounts.txt"), Encoding1251);
      if (Db.Accounts == null) Db.Accounts = new ObservableCollection<Account>();
      foreach (var s in content)
      {
        if (s == "") continue;
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
      var substrings = s.Split(';');
      account.Id = Convert.ToInt32(substrings[0]);
      account.Name = substrings[1].Trim();
      parentId = Convert.ToInt32(substrings[2]);
      return account;
    }

    private static void BuildBranchFromRoot(Account root, string[] content)
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

  }
}
