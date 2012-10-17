﻿using System;
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
  class Restore
  {
    [Import]
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static void RestoreAllTables()
    {
      RestoreCurrencyRates();
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
      int prev = s.IndexOf(',');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, prev));
      prev = s.IndexOf(',', prev + 2);
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(prev + 2, 3));
      prev += 6;
      rate.Rate = Convert.ToDecimal(s.Substring(prev));
      return rate;
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
//          AccountsRoots.Add(account);
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
