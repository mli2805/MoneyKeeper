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
    public List<string> CurrencyList { get; set; }
    public string SelectedCurrency { get; set; }

    public AddAccountViewModel(Account parentAccount)
    {
      AccountUnderCreation = new Account();
      AccountUnderCreation.Parent = parentAccount;

      PrepareCurrencyComboBox();
    }

    private void PrepareCurrencyComboBox()
    {
      CurrencyList = new List<string>();
      CurrencyList.Add("BYR");
      CurrencyList.Add("RUB");
      CurrencyList.Add("USD");
      CurrencyList.Add("EUR");
      SelectedCurrency = CurrencyList[0];
    }

    public void Accept()
    {
      AccountUnderCreation.Parent.Children.Add(AccountUnderCreation);
      TryClose(true);
    }
  }
}
