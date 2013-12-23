using System.Composition;
using Keeper.DomainModel;

namespace Keeper.Utils.Accounts
{
  [Export]
  public class AccountInTreeSeeker
  {
    private readonly KeeperDb _db;

    [ImportingConstructor]
    public AccountInTreeSeeker(KeeperDb db)
    {
      _db = db;
    }

	Account FindAccountInBranch(string toFind, Account branch)
    {
      foreach (var child in branch.Children)
      {
        if (child.Name == toFind) return child;
        var res = FindAccountInBranch(toFind, child);
        if (res != null) return res;
      }
      return null;
    }

    public Account FindAccountInTree(string name)
    {
      foreach (var root in _db.Accounts)
      {
        if (root.Name == name) return root;
        var res = FindAccountInBranch(name, root);
        if (res != null) return res;
      }
      return null;
    }
  }
}