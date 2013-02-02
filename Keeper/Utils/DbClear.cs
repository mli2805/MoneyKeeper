using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class DbClear
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }

    public static void ClearAllTables()
    {
      Db.CurrencyRates.Clear();
      Db.ArticlesAssociations.Clear();
      Db.Transactions.Clear();

      ClearAccounts();
    }

    private static void ClearAccounts()
    {
      var roots = new List<Account>(from account in Db.Accounts
                                    where account.Parent == null
                                    select account);
      foreach (var root in roots)
      {
        RemoveAccountFromDatabase(root);
      }
    }

    public static void RemoveAccountFromDatabase(Account account)
    {
      foreach (var child in account.Children.ToArray())
      {
        RemoveAccountFromDatabase(child);
      }
      Db.Accounts.Remove(account);
    }

  }
}
