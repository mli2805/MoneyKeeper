using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class AddAndEditAccountViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public String Title { get; set; }
    public Account AccountInWork { get; set; }

    public List<Account> CategoriesForParentList { get; set; }
    public Account SelectedParent { get; set; }

    public AddAndEditAccountViewModel(Account account, string title)
    {
      Title = title;
      AccountInWork = account;
      PrepareParentComboBox();
    }

    public Account GetRoot(Account account)
    {
      if (account.Parent == null) return account;
      return GetRoot(account.Parent);
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
      GetBranchFromPoint(root,CategoriesForParentList);
      CategoriesForParentList.Remove(AccountInWork); // не работает, т.к. разные инстансы , надо перегружать операцию сравнения (по Id)

//      CategoriesForParentList = new List<Account>(from account in Db.Categories.Local
//                                                   where account == root
//                                                   select account);
      SelectedParent = AccountInWork.Parent;
    }

    public void Accept()
    {
      AccountInWork.Parent = SelectedParent;
      TryClose(true);
    }


  }
}
