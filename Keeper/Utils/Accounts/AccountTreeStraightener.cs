using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;

namespace Keeper.Utils.Accounts
{
  [Export]
  public class AccountTreeStraightener
  {
    public List<Account> FillInAccountsPlaneList(IEnumerable<Account> roots)
    {
      var result = new List<Account>();
      foreach (var account in roots)
      {
        result.Add(account);
        var childList = FillInAccountsPlaneList(account.Children);
        result.AddRange(childList);
      }
      return result;
    }
  }
}