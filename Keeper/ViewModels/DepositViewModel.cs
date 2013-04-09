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

    public DepositViewModel(Account account)
    {
      Deposit = new Deposit { Account = account };
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = Deposit.Account.Name;
      Deposit.CollectInfo();
    }

    public void Renew()
    {
      WindowManager.ShowDialog(new RenewDepositViewModel(Deposit));
    }



  }
}
