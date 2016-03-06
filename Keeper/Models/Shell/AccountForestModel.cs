using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;

namespace Keeper.Models.Shell
{
  public class AccountForestModel : PropertyChangedBase
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

    private Account _selectedAccount;
    public Account SelectedAccount
    {
      get { return _selectedAccount; }
      set
      {
        _selectedAccount = value;
        DepositEvaluationVisibility = value != null && value.Deposit != null ? Visibility.Visible : Visibility.Collapsed;
        AddNewAccountVisibility = value != null && value.IsFolder ? Visibility.Visible : Visibility.Collapsed;
        NotifyOfPropertyChange(()=>SelectedAccount);
      }
    }

    private int _openedAccountPage;
    public int OpenedAccountPage
    {
      get { return _openedAccountPage; }
      set
      {
        _openedAccountPage = value;
        SelectedAccount = FindSelectedOrAssignFirstAccountOnPage(_openedAccountPage);
        SelectedAccount.IsSelected = true;
        NotifyOfPropertyChange(()=>OpenedAccountPage);
      }
    }

    private Visibility _depositEvaluationVisibility;

    public Visibility DepositEvaluationVisibility
    {
      get { return _depositEvaluationVisibility; }
      set
      {
        if (value.Equals(_depositEvaluationVisibility)) return;
        _depositEvaluationVisibility = value;
        NotifyOfPropertyChange(() => DepositEvaluationVisibility);
      }
    }

    private Visibility _addNewAccountVisibility;
    public Visibility AddNewAccountVisibility
    {
      get { return _addNewAccountVisibility; }
      set
      {
        if (Equals(value, _addNewAccountVisibility)) return;
        _addNewAccountVisibility = value;
        NotifyOfPropertyChange(() => AddNewAccountVisibility);
      }
    }
  }
}