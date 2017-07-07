using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.CommonKeeper
{
  [Export]
  public class DbCleaner
  {

    public void ClearAllTables(KeeperDb db)
    {
      db.CurrencyRates.Clear();
      db.ArticlesAssociations.Clear();
      db.TransWithTags.Clear();

      ClearAccounts(db);
    }

    private void ClearAccounts(KeeperDb db)
    {
      var roots = new List<Account>(from account in db.Accounts
                                    where account.Parent == null
                                    select account);
      foreach (var root in roots)
      {
        RemoveBranchFromDatabase(db, root);
      }
    }

    public void RemoveBranchFromDatabase(KeeperDb db, Account branch)
    {
      foreach (var child in branch.Children.ToArray())
      {
        RemoveBranchFromDatabase(db, child);
      }
      db.Accounts.Remove(branch);
    }
  }
}
