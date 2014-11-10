using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using System.Linq;

namespace Keeper.ByFunctional.EditingAccounts
{
  [Export]
  public class AccountTreeStraightener
  {
    public Account Seek(string name, IEnumerable<Account> accounts)
    {
      return Flatten(accounts).FirstOrDefault(a => a.Name == name);
    }

    public IEnumerable<Account> Flatten(IEnumerable<Account> roots)
    {
      return FlattenWithLevels(roots).Select(a => a.Item);
    }

    public IEnumerable<HierarchyItem<Account>> FlattenWithLevels(IEnumerable<Account> accounts)
    {
      return accounts.SelectMany(accountsRoot => RecursiveWalk(accountsRoot, 0));
    }

    private static IEnumerable<HierarchyItem<Account>> RecursiveWalk(Account account, int depth)
    {
      yield return new HierarchyItem<Account>(depth, account);
      foreach (var hierarchyItem in account.Children.SelectMany(child => RecursiveWalk(child, depth + 1)))
        yield return hierarchyItem;
    }

  }

}