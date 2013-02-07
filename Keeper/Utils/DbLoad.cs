using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  class DbLoad
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);
    public static Dictionary<DateTime, Decimal> MonthRashod { get; set; }


    public static TimeSpan LoadAllTables()
    {
      var start = DateTime.Now;

      LoadCurrencyRates();
      LoadAccounts();
      if (Db.AccountsPlaneList == null) Db.AccountsPlaneList = new List<Account>(); else Db.AccountsPlaneList.Clear();
      FillInAccountsPlaneList(Db.Accounts);
      LoadTransactions();
      LoadArticlesAssociations();

      return DateTime.Now - start;
    }

    #region // 2002-2010

    public class Rashod2002
    {
      public DateTime Dt;
      public decimal InUsdRuki;
      public decimal InUsdVklad;
      public decimal Rate;
      public decimal InByr;
      public string Comment;

      public new string ToString()
      {
        return String.Format("{0} ; {1} ; {2}  ; {3} ; {4} ; {5}", Dt, InByr, Rate, InUsdRuki, InUsdVklad, Comment);
      }
    }

    public static void StartingBalances()
    {
      LoadTransactionsFrom("Transactions ввод остатков 1 января 2002.txt");
    }

    /// <summary>
    /// заполняет белорусские и курс там где они не стояли
    /// суммирует расходы за месяц и сумму в белорусских с датой заносит в словарик
    /// </summary>
    public static void Make2002Normal()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "TransactionsFromRashods.txt"), Encoding1251);
      var contentOut = new List<string>();
      var currentMonthDate = new DateTime(0);
      decimal currentMonthSum = 0;
      foreach (var s in content)
      {
        var rashod = ParseRashod2002(s);
        if (rashod.Rate == 0) rashod.Rate = (decimal)Rate.GetRate(CurrencyCodes.BYR, rashod.Dt);
        if (rashod.InByr == 0)
          rashod.InByr = (rashod.InUsdRuki + rashod.InUsdVklad) * rashod.Rate;

        contentOut.Add(rashod.ToString());

        if (currentMonthDate == rashod.Dt) currentMonthSum += rashod.InByr;
        else
        {
          if (MonthRashod == null) MonthRashod = new Dictionary<DateTime, decimal>();
          MonthRashod.Add(currentMonthDate, currentMonthSum);
          currentMonthDate = rashod.Dt;
          currentMonthSum = rashod.InByr;
        }
      }
      MonthRashod.Add(currentMonthDate, currentMonthSum);
      File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "TransactionsFromRashodsNormal.txt"), contentOut, Encoding1251);
    }

    private static Rashod2002 ParseRashod2002(string s)
    {
      var result = new Rashod2002();
      var substrings = s.Split(';');

      result.Dt = Convert.ToDateTime(substrings[0]);
      // пропуск слова Расход
      var ruki = substrings[2].Trim();
      result.InByr = ruki != "" ? Convert.ToDecimal(ruki) : 0;
      string rate = substrings[3].Trim();
      result.Rate = rate != "" ? Convert.ToDecimal(rate) : 0;
      result.InUsdRuki = Convert.ToDecimal(substrings[4]);
      result.InUsdVklad = Convert.ToDecimal(substrings[5]);
      result.Comment = substrings[6].Trim();
      return result;
    }

    /// <summary>
    /// разбирает файл доходов , заносит доходы в транзакции
    /// </summary>
    public static void Load2002D()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "TransactionsFromDohods.txt"), Encoding1251);
      if (Db.Transactions == null) Db.Transactions = new ObservableCollection<Transaction>();
      var wrongContent = new List<string>();
      foreach (var s in content)
      {
        if (s == "") continue;

        Transaction transaction = null;
        try
        {
          transaction = TransactionFrom2002D(s);
        }
        catch (Exception)
        {
          wrongContent.Add(s);
        }
        if (transaction != null) Db.Transactions.Add(transaction);
      }
      if (wrongContent.Count != 0) File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "Load2002D.err"), wrongContent, Encoding1251);
    }

    private static Transaction TransactionFrom2002D(string s)
    {
      var transaction = new Transaction
      {
        Timestamp = Convert.ToDateTime(s.Substring(0, 10)).AddHours(9),
        Operation = OperationType.Доход
      };

      var prev = 21;
      var next = s.IndexOf(';', prev);
      var debet = s.Substring(prev, next - prev - 1);
      transaction.Debet = Db.AccountsPlaneList.First(account => account.Name == debet);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      var credit = s.Substring(prev + 2, next - prev - 3);
      transaction.Credit = Db.AccountsPlaneList.First(account => account.Name == credit);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      var article = s.Substring(prev + 2, next - prev - 3);
      transaction.Article = Db.AccountsPlaneList.First(account => account.Name == article);
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Amount = Convert.ToDecimal(s.Substring(prev + 2, next - prev - 3));
      prev = next;
      next = s.IndexOf(';', prev + 2);
      transaction.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(prev + 2, next - prev - 3));

      transaction.Amount2 = 0;
      transaction.Currency2 = null;
      transaction.Comment = s.Substring(next + 2);

      return transaction;
    }

    /// <summary>
    /// загрузка расходов из распределения по категориям
    /// </summary>
    public static void Load2002Rk()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "TransactionsFromRashodsKategories.txt"), Encoding1251);
      if (Db.Transactions == null) Db.Transactions = new ObservableCollection<Transaction>();
      var wrongContent = new List<string>();
      foreach (var s in content)
      {
        if (s == "") continue;

        try
        {
          Parse2002Rk(s);
        }
        catch (Exception)
        {
          wrongContent.Add(s);
        }
      }
      if (wrongContent.Count != 0) File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "Load2002R.err"), wrongContent, Encoding1251);
    }

    /// <summary>
    /// разбирает строку одного месяца, 
    /// сумму сумм категорий сравнивает с общим расходом из словарика и пропорционально изменяет
    /// </summary>
    /// <param name="s"></param>
    private static void Parse2002Rk(string s)
    {
      DateTime dt = Convert.ToDateTime("15" + s.Substring(12, 8)).AddHours(10);
      var articles = new Account[9];
      var amounts = new decimal[9];
      articles[0] = Db.AccountsPlaneList.First(account => account.Name == "Продукты в целом");
      articles[1] = Db.AccountsPlaneList.First(account => account.Name == "Автомобиль");
      articles[2] = Db.AccountsPlaneList.First(account => account.Name == "Лекарства");
      articles[3] = Db.AccountsPlaneList.First(account => account.Name == "Квартира1");
      articles[4] = Db.AccountsPlaneList.First(account => account.Name == "Квартира2");
      articles[5] = Db.AccountsPlaneList.First(account => account.Name == "Одежда");
      articles[6] = Db.AccountsPlaneList.First(account => account.Name == "Ремонт");
      articles[7] = Db.AccountsPlaneList.First(account => account.Name == "Дача");
      articles[8] = Db.AccountsPlaneList.First(account => account.Name == "Прочие расходы");

      var prev = 28;
      decimal allAmounts = 0;
      for (var i = 0; i < 9; i++)
      {
        var next = s.IndexOf(';', prev + 2);
        amounts[i] = Convert.ToDecimal(s.Substring(prev + 2, next - prev - 3));
        allAmounts += amounts[i];
        prev = next;
      }

      // один раз курсы загрузил и хватит!

//      var last = s.IndexOf(';', prev + 2);
//      var rate = new CurrencyRate
//                   {
//                     BankDay = dt,
//                     Currency = CurrencyCodes.BYR,
//                     Rate = Convert.ToDouble(s.Substring(prev + 2, last - prev - 3))
//                   };
//      Db.CurrencyRates.Add(rate);
      Db.Transactions.Add(new Transaction
                            {
                              Timestamp = dt.Date.AddHours(9).AddMinutes(30),
                              Operation = OperationType.Обмен,
                              Debet = Db.AccountsPlaneList.First(account => account.Name == "Мой кошелек"),
                              Credit = Db.AccountsPlaneList.First(account => account.Name == "обменник"),
                              Amount = 100,
                              Currency = CurrencyCodes.USD,
                              Amount2 = 100,
                              Currency2 = CurrencyCodes.BYR,
                              Article = null,
                              Comment = "Все расходы в рублях, часть доходов в долларах, часть меняем"
                            });

      decimal montsAmount;
      MonthRashod.TryGetValue(dt.Date, out montsAmount);
      for (var i = 0; i < 9; i++)
      {
        if (amounts[i] != 0)
          Db.Transactions.Add(Common2002Rk(dt.AddMinutes(i), amounts[i] * montsAmount / allAmounts, articles[i]));
      }
    }

    private static Transaction Common2002Rk(DateTime dt, decimal amount, Account article)
    {
      var transaction = new Transaction
                          {
                            Timestamp = dt,
                            Operation = OperationType.Расход,
                            Debet = Db.AccountsPlaneList.First(account => account.Name == "Мой кошелек"),
                            Credit = Db.AccountsPlaneList.First(account => account.Name == "Прочие магазины"),
                            Amount = amount,
                            Currency = CurrencyCodes.BYR,
                            Amount2 = 0,
                            Currency2 = null,
                            Article = article,
                            Comment = ""
                          };
      return transaction;
    }

    #endregion

    #region // Transactions

    private static void LoadTransactions()
    {
      LoadTransactionsFrom("Transactions.txt");
    }

    private static void LoadTransactionsFrom(string filename)
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, filename), Encoding1251);
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
      if (wrongContent.Count != 0)
      {
        File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "LoadTransactions.err"), wrongContent, Encoding1251);
        MessageBox.Show("Ошибки загрузки транзакций смотри в файле LoadTransactions.err");
      }
    }

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
      transaction.Comment = substrings[9];

      return transaction;
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
      account.IsExpanded = Convert.ToBoolean(substrings[3]);
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

    public static void FillInAccountsPlaneList(IEnumerable<Account> accountsList)
    {
      foreach (var account in accountsList)
      {
        Db.AccountsPlaneList.Add(account);
        FillInAccountsPlaneList(account.Children);
      }
    }

    public static int GetMaxAccountId()
    {
      return (from account in Db.AccountsPlaneList select account.Id).Max();
    }

    #endregion

    #region // Currency Rates
    public static void LoadCurrencyRates()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.SavePath, "CurrencyRates.txt"), Encoding1251);
      if (Db.CurrencyRates == null) Db.CurrencyRates = new ObservableCollection<CurrencyRate>();
      var wrongContent = new List<string>();
      foreach (var s in content)
      {
        if (s == "") continue;

        CurrencyRate rate = null;
        try
        {
          rate = CurrencyRateFromString(s);
        }
        catch (Exception)
        {
          wrongContent.Add(s);
        }
        if (rate != null) Db.CurrencyRates.Add(rate);
        if (wrongContent.Count != 0)
        {
          File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "CurrencyRates.err"), wrongContent, Encoding1251);
          MessageBox.Show("Ошибки загрузки курсов валют смотри в файле CurrencyRates.err");
        }
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
      if (wrongContent.Count != 0)
      {
        File.WriteAllLines(Path.Combine(Settings.Default.SavePath, "LoadArticlesAssociations.err"), wrongContent, Encoding1251);
        MessageBox.Show("Ошибки загрузки ассоциаций смотри в файле LoadArticlesAssociations.err");
      }
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
