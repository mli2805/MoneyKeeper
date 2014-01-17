using System;
using System.Composition;
using System.Windows;
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

    public MainMenuModel MyMainMenuModel { get; set; }
    public AccountForestModel MyForestModel { get; set; }
    public BalanceListModel MyBalanceListModel { get; set; }
    public TwoSelectorsModel MyTwoSelectorsModel { get; set; }
    public StatusBarModel MyStatusBarModel { get; set; }

    [ImportingConstructor]
    public ShellModel(BalancesForShellCalculator balancesForShellCalculator)
    {
      _balancesForShellCalculator = balancesForShellCalculator;

      MyMainMenuModel = new MainMenuModel();
      MyMainMenuModel.PropertyChanged += MyMainMenuModelPropertyChanged;
      MyForestModel = new AccountForestModel();
      MyForestModel.PropertyChanged += MyForestModelPropertyChanged;
      MyBalanceListModel = new BalanceListModel();
      MyTwoSelectorsModel = new TwoSelectorsModel();
      MyTwoSelectorsModel.PropertyChanged += MyTwoSelectorsModelPropertyChanged;
      MyStatusBarModel = new StatusBarModel();
    }

    void MyMainMenuModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Action")
      {
        switch (MyMainMenuModel.Action)
        {
          case Actions.SaveWithProgressBar:
            MyStatusBarModel.Item0 = "Сохранение данных на диск";
            MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
            break;
          case Actions.PreparingExit:
            MyStatusBarModel.Item0 = "Идет завершение программы...";
            MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
            break;
          case Actions.Quit:
            break;
          default: return;
        }
      }
    }

    void MyTwoSelectorsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "TranslatedPeriod")
        MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.CountBalances(
          MyForestModel.SelectedAccount, MyTwoSelectorsModel.TranslatedPeriod, MyBalanceListModel.BalanceList));

      if (e.PropertyName == "TranslatedDate")
        MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.CountBalances(
          MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate), MyBalanceListModel.BalanceList));

    }

    void MyForestModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SelectedAccount")
      {
        MyBalanceListModel.Caption = MyForestModel.SelectedAccount.Name;
        if (MyTwoSelectorsModel.IsPeriodMode)
        {
          MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.CountBalances(
            MyForestModel.SelectedAccount, MyTwoSelectorsModel.TranslatedPeriod, MyBalanceListModel.BalanceList));
        }
        else
        {
          MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.CountBalances(
            MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate), MyBalanceListModel.BalanceList));
        }
      }

      if (e.PropertyName == "OpenedAccountPage")
        MyTwoSelectorsModel.IsPeriodMode = MyForestModel.OpenedAccountPage != 0;
    }



  }
}
