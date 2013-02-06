using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    public Deposit SelectedDeposit { get; set; }
    public List<DepositViewModel> LaunchedViews { get; set; }

    public DepositsViewModel()
    {
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
      TotalsList = new List<string>{"Итоговые показатели:"};
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Депозиты";
    }

    public override void CanClose(System.Action<bool> callback)
    {
      if (LaunchedViews != null)
        foreach (var depositViewModel in LaunchedViews)
          if (depositViewModel.Alive) depositViewModel.TryClose();
      base.CanClose(callback);
    }

    public void ShowSelectedDeposit()
    {
      if (LaunchedViews == null) LaunchedViews = new List<DepositViewModel>();
      var depositViewModel = new DepositViewModel(SelectedDeposit.Account);
      LaunchedViews.Add(depositViewModel);
      WindowManager.ShowWindow(depositViewModel);
    }
  }
}
