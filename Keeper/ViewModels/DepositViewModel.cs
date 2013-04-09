using System.ComponentModel.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class DepositViewModel : Screen
  {
    [Import]
    public IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

    public Deposit Deposit { get; set; }
    public Account NewAccountForDeposit { get; set; }

    public DepositViewModel(Account account)
    {
      Deposit = new Deposit { Account = account };
      NewAccountForDeposit = null;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = Deposit.Account.Name;
      Deposit.CollectInfo();
    }

    public void Renew()
    {
      var renewDepositViewModel = new RenewDepositViewModel(Deposit);
      WindowManager.ShowDialog(renewDepositViewModel);
      if (renewDepositViewModel.NewDeposit != null)
      {
        NewAccountForDeposit = renewDepositViewModel.NewDeposit;
        Deposit.CollectInfo();
      }
    }



  }
}
