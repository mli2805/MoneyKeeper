using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class DepositsViewModel : Screen
  {
    public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public List<Deposit> DepositsList { get; set; }
    public List<string> TotalsList { get; set; }
    public List<string> YearsList { get; set; }
    public Deposit SelectedDeposit { get; set; }
    public List<DepositViewModel> LaunchedViews { get; set; }
    public bool Alive { get; set; }

    public DepositsViewModel()
    {
      Alive = true;
      DepositsList = new List<Deposit>();
      foreach (var account in Db.AccountsPlaneList)
      {
        if (account.IsDescendantOf("Депозиты") && account.Children.Count == 0)
        {
          var temp = new Deposit {Account = account};
          temp.CollectInfo();
          DepositsList.Add(temp);
        }
      }
      SelectedDeposit = DepositsList[0];
      TotalBalances();
      YearsProfit();
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Депозиты";
    }

    public override void CanClose(Action<bool> callback)
    {
      if (LaunchedViews != null)
        foreach (var depositViewModel in LaunchedViews)
          if (depositViewModel.Alive) depositViewModel.TryClose();
      Alive = false;
      base.CanClose(callback);
    }

    public void ShowSelectedDeposit()
    {
      if (LaunchedViews == null) LaunchedViews = new List<DepositViewModel>();
      var depositViewModel = new DepositViewModel(SelectedDeposit.Account);
      LaunchedViews.Add(depositViewModel);
      WindowManager.ShowWindow(depositViewModel);
    }

    public void TotalBalances()
    {
      TotalsList = new List<string> { "Сумма депозитов на текущий момент:\n" };
      var totalBalances = new Dictionary<CurrencyCodes, decimal>();

      foreach (var deposit in DepositsList)
      {
        if (deposit.CurrentBalance == 0) continue;
        decimal total;
        if (totalBalances.TryGetValue(deposit.MainCurrency, out total))
          totalBalances[deposit.MainCurrency] = total + deposit.CurrentBalance;
        else
          totalBalances.Add(deposit.MainCurrency,deposit.CurrentBalance);
      }

      foreach (var currency in totalBalances.Keys)
      {
        TotalsList.Add(String.Format("{0:#,0} {1}",totalBalances[currency],currency));
      }
    }

    public void YearsProfit()
    {
      YearsList = new List<string> { "Суммы дохода по годам начисления (не выплаты):\n" };
      for (int i = 2002; i < DateTime.Today.Year; i++)
      {
        decimal yearTotal = 0;
        foreach (var deposit in DepositsList)
        {
          yearTotal += deposit.GetProfitForYear(i);
        }
        if (yearTotal != 0) YearsList.Add(String.Format("   {0} год  -   {1:#,0} usd,   в месяц примерно {2:#,0} usd", i, yearTotal, yearTotal/12));
      }
    }

  }
}
