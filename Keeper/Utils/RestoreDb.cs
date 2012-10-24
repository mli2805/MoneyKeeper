using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils
{
  class RestoreDb
  {
    [Import]
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static void RestoreAllTables()
    {
      RestoreCurrencyRates();
      RestoreCategories();
      RestoreAccounts();
    }

    public static void RestoreCurrencyRates()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "CurrencyRates.txt"));
      foreach (var s in content)
      {
        var rate = CurrencyRateFromString(s);
        Db.CurrencyRates.Add(rate);
      }
    }

    private static CurrencyRate CurrencyRateFromString(string s)
    {
      var rate = new CurrencyRate();
      int next = s.IndexOf(',');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, next));
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(next + 2, 3));
      next += 6;
      rate.Rate = Convert.ToDecimal(s.Substring(next+2));
      return rate;
    }

    public static void RestoreCategories()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "Categories.txt"));
      foreach (var s in content)
      {
        int parentId;
        var category = CategoryFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(category, content);
          Db.Categories.Add(category);
        }
      }
    }

    private static Category CategoryFromString(string s, out int parentId)
    {
      var category = new Category();
      int prev = s.IndexOf(',');
      category.Id = Convert.ToInt32(s.Substring(0, prev));
      int next = s.IndexOf(',', prev + 2);
      category.Name = s.Substring(prev + 2, next - prev - 3);
      parentId = Convert.ToInt32(s.Substring(next + 2));
      return category;
    }

    private static void BuildBranchFromRoot(Category root, string[] content)
    {
      foreach (var s in content)
      {
        int parentId;
        var category = CategoryFromString(s, out parentId);
        if (parentId == root.Id)
        {
          category.Parent = root;
          root.Children.Add(category);
          BuildBranchFromRoot(category, content);
        }
      }
    }

    public static void RestoreAccounts()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "Accounts.txt"));
      foreach (var s in content)
      {
        int parentId;
        var account = AccoutFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, content);
          Db.Accounts.Add(account);
        }
      }
    }

    private static Account AccoutFromString(string s, out int parentId)
    {
      var account = new Account();
      int prev = s.IndexOf(',');
      account.Id = Convert.ToInt32(s.Substring(0, prev));
      int next = s.IndexOf(',', prev + 2);
      account.Name = s.Substring(prev + 2, next - prev - 3);
      prev = next;
      account.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(prev + 2, 3));
      prev += 6;
      next = s.IndexOf(',', prev + 2);
      parentId = Convert.ToInt32(s.Substring(prev + 2, next - prev - 3));
      prev = next;
      account.IsAggregate = Convert.ToBoolean(s.Substring(prev + 2));
      return account;
    }

    private static void BuildBranchFromRoot(Account root, string[] content)
    {
      foreach (var s in content)
      {
        int parentId;
        var account = AccoutFromString(s, out parentId);
        if (parentId == root.Id)
        {
          account.Parent = root;
          root.Children.Add(account);
          BuildBranchFromRoot(account, content);
        }
      }
    }

  }
}
