using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
  class DbCleaner
  {

    public void ClearAllTables(KeeperDb db)
    {
      db.CurrencyRates.Clear();
      db.ArticlesAssociations.Clear();
      db.Transactions.Clear();

      ClearAccounts(ref db);
    }

    private void ClearAccounts(ref KeeperDb db)
    {
      var roots = new List<Account>(from account in db.Accounts
                                    where account.Parent == null
                                    select account);
      foreach (var root in roots)
      {
        RemoveBranchFromDatabase(ref db, root);
      }
    }

    public void RemoveBranchFromDatabase(ref KeeperDb db, Account branch)
    {
      foreach (var child in branch.Children.ToArray())
      {
        RemoveBranchFromDatabase(ref db, child);
      }
      db.Accounts.Remove(branch);
    }
  }
}
