using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class AccountViewModel:Screen
  {
    public Account AccountInWork { get; set; }
    public FormMode CurrentMode { get; set; }
    public List<CurrencyCodes> CurrencyList { get; set; }
    public CurrencyCodes SelectedCurrency { get; set; }

    public void PrepareForCreate(Account parentAccount)
    {
      AccountInWork = new Account();
      AccountInWork.Parent = parentAccount;
    }

    public void PrepareForEdit(Account editingAccount)
    {
      AccountInWork = editingAccount;
    }

    public AccountViewModel(Account account, FormMode mode)
    {
      PrepareCurrencyComboBox();
      CurrentMode = mode;
      if (mode == FormMode.Create) PrepareForCreate(account);
      else if (mode == FormMode.Edit) PrepareForEdit(account);
      else MessageBox.Show("Form mode can't be " + mode, "Something is wrong!");
    }

    private void PrepareCurrencyComboBox()
    {
      CurrencyList = Enum.GetValues(typeof (CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      SelectedCurrency = CurrencyList[0];
    }

    public void Accept()
    {
      AccountInWork.Currency = SelectedCurrency;
      if (CurrentMode == FormMode.Create)
        AccountInWork.Parent.Children.Add(AccountInWork);
      TryClose(true);
    }
  }
}
