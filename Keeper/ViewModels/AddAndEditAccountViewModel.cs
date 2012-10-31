using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class AddAndEditAccountViewModel:Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public String Title { get; set; }
    public Account AccountInWork { get; set; }

    public List<CurrencyCodes> CurrencyList { get; private set; }
    public CurrencyCodes SelectedCurrency { get; set; }

    public List<Account> AccountsForParentList { get; set; }
    public Account SelectedParent { get; set; }

    public bool IsAggregateCombobox { get; set; }

    public AddAndEditAccountViewModel(Account account, string title)
    {
      Title = title;
      AccountInWork = account;
      IsAggregateCombobox = AccountInWork.IsAggregate;
      PrepareCurrencyComboBox();
      PrepareParentComboBox();
    }

    private void PrepareCurrencyComboBox()
    {
      CurrencyList = Enum.GetValues(typeof (CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      SelectedCurrency = AccountInWork.Currency;
    }

    private void PrepareParentComboBox()
    {
      AccountsForParentList = new List<Account>(from account in Db.Accounts.Local
                                                        where account != AccountInWork
                                                        select account);
      var worldRoot = new Account("World Root", true);
      AccountsForParentList.Add(worldRoot);
      if (AccountInWork.Parent != null) SelectedParent = AccountInWork.Parent;
      else SelectedParent = worldRoot;
    }

    public void Accept()
    {
      // TODO вместо отдельных свойств прибиндиться к полям Account
      AccountInWork.Currency = SelectedCurrency;
      AccountInWork.IsAggregate = IsAggregateCombobox;
 //     AccountInWork.Parent = SelectedParent.Id == 0 ? null : SelectedParent; // если Id = 0 то присвоить null иначе можно SelectedParent
      AccountInWork.Parent = SelectedParent;
      TryClose(true);
    }
  }
}
