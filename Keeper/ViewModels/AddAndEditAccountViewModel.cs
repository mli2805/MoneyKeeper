using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;

namespace Keeper.ViewModels
{
  public class AddAndEditAccountViewModel : Screen
  {
    public String ViewTitle { get; set; }
    public Account AccountInWork { get; set; }

    public List<Account> CategoriesForParentList { get; set; }
    public Account SelectedParent { get; set; }

    public AddAndEditAccountViewModel(Account account, string viewTitle)
    {
      ViewTitle = viewTitle;
      AccountInWork = account;
      PrepareParentComboBox();
    }

    public Account GetRoot(Account account)
    {
      return account.Parent == null ? account : GetRoot(account.Parent);
    }

    public void GetBranchFromPoint(Account point, List<Account> list)
    {
      list.Add(point);
      foreach (var child in point.Children)
      {
        GetBranchFromPoint(child, list);
      }
    }

    private void PrepareParentComboBox()
    {
      var root = GetRoot(AccountInWork);
      CategoriesForParentList = new List<Account>();
      GetBranchFromPoint(root, CategoriesForParentList);
      CategoriesForParentList.Remove(AccountInWork); // не работает, т.к. разные инстансы , надо перегружать операцию сравнения (по Id)
      SelectedParent = AccountInWork.Parent;
    }

    public void Accept()
    {
      AccountInWork.Parent = SelectedParent;
      TryClose(true);
    }

    public void ConvertToDeposit()
    {
      AccountInWork.Deposit = new Deposit();
      TryClose(true);
    }
  }
}
