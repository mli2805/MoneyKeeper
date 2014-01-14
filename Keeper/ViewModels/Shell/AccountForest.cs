using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ViewModels.Shell
{
  public class AccountForest
  {
    public ObservableCollection<Account> MineAccountsRoot { get; set; }
    public ObservableCollection<Account> ExternalAccountsRoot { get; set; }
    public ObservableCollection<Account> IncomesRoot { get; set; }
    public ObservableCollection<Account> ExpensesRoot { get; set; }

    private Account GetSelectedInBranch(Account account)
    {
      if (account.IsSelected) return account;
      foreach (var child in account.Children)
      {
        var result = GetSelectedInBranch(child);
        if (result != null) return result;
      }
      return null;
    }

    private Account GetSelectedInCollection(IEnumerable<Account> roots)
    {
      foreach (var branch in roots)
      {
        var result = GetSelectedInBranch(branch);
        if (result != null) return result;
      }
      return null;
    }

    private ObservableCollection<Account> GetRootByNumber(int pageNumber)
    {
      switch (pageNumber)
      {
        case 1: return ExternalAccountsRoot;
        case 2: return IncomesRoot;
        case 3: return ExpensesRoot;
        default: return MineAccountsRoot;
      }
    }

    public Account FindSelectedOrAssignFirstAccountOnPage(int pageNumber)
    {
      var collection = GetRootByNumber(pageNumber);
      var result = GetSelectedInCollection(collection);
      if (result == null && collection.Count != 0)
      {
        result = (from account in collection select account).First();
        result.IsSelected = true;
      }
      return result;
    }
  }
}