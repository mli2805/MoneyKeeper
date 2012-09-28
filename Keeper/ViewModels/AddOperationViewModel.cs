using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class AddOperationViewModel : Screen
  {
    private readonly List<Account> _accountsList;

    public List<Account> AccountsList
    {
      get { return _accountsList; }
    }

    public Account SelectedAccount { get; set; }

    public List<CurrencyCodes> CurrencyList
    {
      get { return _currencyList; }
    }

    private List<CurrencyCodes> _currencyList;
    private Visibility _exchangeVisibility;
    private uint _formBackground;

    public Visibility ExchangeVisibility
    {
      get { return _exchangeVisibility; }
      set
      {
        if (Equals(value, _exchangeVisibility)) return;
        _exchangeVisibility = value;
        NotifyOfPropertyChange(() => ExchangeVisibility);
      }
    }

    public uint FormBackground
    {
      get { return _formBackground; }
      set
      {
        if (value == _formBackground) return;
        _formBackground = value;
        NotifyOfPropertyChange(() => FormBackground);
      }
    }

    public void ChangeFormForIncome()
    {
      ExchangeVisibility = Visibility.Hidden;
      FormBackground = 0xFFF7F7FF;
    }


    public AddOperationViewModel()
    {
      _accountsList = new List<Account>();

      _accountsList.Clear();

      _accountsList.Add(new Account("11111"));
      _accountsList.Add(new Account("222222"));
      SelectedAccount = _accountsList[0];
      FormBackground = 0xFFFFFFFF;

    }

  }
}
