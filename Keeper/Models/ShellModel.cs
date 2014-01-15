using System;
using System.Composition;
using Keeper.DomainModel;
using Keeper.Utils.Balances;
using Keeper.ViewModels.Shell;

namespace Keeper.Models
{
  [Export]
  [Shared]
  public class ShellModel
  {
    private readonly BalancesForShellCalculator _balancesForShellCalculator;

    public AccountForestModel MyForestModel { get; set; }
    public BalanceListModel MyBalanceListModel { get; set; }
    public TwoSelectorsModel MyTwoSelectorsModel { get; set; }

    [ImportingConstructor]
    public ShellModel(BalancesForShellCalculator balancesForShellCalculator)
    {
      _balancesForShellCalculator = balancesForShellCalculator;

      MyForestModel = new AccountForestModel();
      MyForestModel.PropertyChanged += MyForestModel_PropertyChanged;
      MyBalanceListModel = new BalanceListModel();
      MyTwoSelectorsModel = new TwoSelectorsModel();
      MyTwoSelectorsModel.PropertyChanged += MyTwoSelectorsModel_PropertyChanged;
    }

    void MyTwoSelectorsModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "TranslatedPeriod")
        MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd",_balancesForShellCalculator.CountBalances(
          MyForestModel.SelectedAccount, MyTwoSelectorsModel.TranslatedPeriod, MyBalanceListModel.BalanceList));

      if (e.PropertyName == "TranslatedDate")
        MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd",_balancesForShellCalculator.CountBalances(
          MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate), MyBalanceListModel.BalanceList));

    }

    void MyForestModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SelectedAccount")
        if (MyTwoSelectorsModel.IsPeriodMode)
        {
          MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd",
                                                                 _balancesForShellCalculator.CountBalances(
                                                                   MyForestModel.SelectedAccount,
                                                                   MyTwoSelectorsModel.TranslatedPeriod,
                                                                   MyBalanceListModel.BalanceList));
        }
        else
        {
          MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.CountBalances(
            MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate), MyBalanceListModel.BalanceList));
        }


      if (e.PropertyName == "OpenedAccountPage")
        MyTwoSelectorsModel.IsPeriodMode = MyForestModel.OpenedAccountPage != 0;
    }



  }
}
