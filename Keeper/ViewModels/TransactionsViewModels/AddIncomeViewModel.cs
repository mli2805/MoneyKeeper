using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class AddIncomeViewModel : Screen
  {

    private readonly List<Account> _accountsList = new List<Account>();
    public List<Account> AccountsList
    {
      get { return _accountsList; }
    }

    public Account SelectedAccount { get; set; }

    private List<CurrencyCodes> _currencyList = new List<CurrencyCodes>();
    public List<CurrencyCodes> CurrencyList
    {
      get { return _currencyList; }
    }

    private Visibility _exchangeVisibility;
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

    private Brush _formBackground;
    public Brush FormBackground
    {
      get { return _formBackground; }
      set
      {
        if (Equals(value, _formBackground)) return;
        _formBackground = value;
        NotifyOfPropertyChange(() => FormBackground);
      }
    }

    public void ChangeFormForIncome()
    {
      ExchangeVisibility = Visibility.Hidden;
      FormBackground =  new SolidColorBrush(Color.FromArgb(0xff,0xff,0xf7,0xf7));
    }

    public void ChangeFormForExpense()
    {
      ExchangeVisibility = Visibility.Hidden;
//      FormBackground = 0xFFF7F7F;
    }

    public void ChangeFormForFundsFlow()
    {
      ExchangeVisibility = Visibility.Visible;
//      FormBackground = 0xFFF7F7FF;
    }


    private void PrepareAccountComboBox()
    {
      _accountsList.Clear();
      _accountsList.Add(new Account("11111"));
      _accountsList.Add(new Account("222222"));
      SelectedAccount = _accountsList[0];
    }

    public AddIncomeViewModel()
    {
      PrepareAccountComboBox();
    }

  }
}
