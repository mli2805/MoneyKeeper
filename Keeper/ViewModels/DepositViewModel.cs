using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class DepositViewModel : Screen
  {
    public Deposit Deposit { get; set; }
    public bool Alive { get; set; }

    public DepositViewModel(Account account)
    {
      Alive = true;
      Deposit = new Deposit { Account = account };
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = Deposit.Account.Name;
      Deposit.CollectInfo();
    }

    public override void CanClose(System.Action<bool> callback)
    {
      Alive = false;
      base.CanClose(callback);
    }

  }
}
