using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.DbInputOutput
{
  class DbClear
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

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
        RemoveBranchFromDatabase(root);
      }
    }

    public static void RemoveBranchFromDatabase(Account branch)
    {
      foreach (var child in branch.Children.ToArray())
      {
        RemoveBranchFromDatabase(child);
      }
      Db.Accounts.Remove(branch);
    }
  }
}
