using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class DepositViewModel : Screen
  {
    public Deposit Deposit { get; set; }

    public DepositViewModel(Account account)
    {
      Deposit = new Deposit { Account = account };
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = Deposit.Account.Name;
      Deposit.MakeReport();
    }

  }
}
