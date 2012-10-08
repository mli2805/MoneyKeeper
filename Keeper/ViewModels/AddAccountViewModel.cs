using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class AddAccountViewModel:Screen
  {
    public Account AccountUnderCreation { get; set; }
    public List<CurrencyCodes> CurrencyList { get; set; }
    public CurrencyCodes SelectedCurrency { get; set; }

    public AddAccountViewModel(Account parentAccount)
    {
      AccountUnderCreation = new Account();
      AccountUnderCreation.Parent = parentAccount;

      PrepareCurrencyComboBox();
    }

    private void PrepareCurrencyComboBox()
    {
      CurrencyList = Enum.GetValues(typeof (CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      SelectedCurrency = CurrencyList[0];
    }

    public void Accept()
    {
      AccountUnderCreation.Currency = SelectedCurrency;
      AccountUnderCreation.Parent.Children.Add(AccountUnderCreation);
      TryClose(true);
    }
  }
}
