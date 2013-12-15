using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class DepositViewModel : Screen
  {
    private bool _canRenew;

    [Import]
    public IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

    private readonly KeeperDb _db;
    public Deposit Deposit { get; set; }
    public Account NewAccountForDeposit { get; set; }
    public bool CanRenew
    {
      get { return _canRenew; }
      set
      {
        if (value.Equals(_canRenew)) return;
        _canRenew = value;
        NotifyOfPropertyChange(() => CanRenew);
      }
    }

    public DepositViewModel(KeeperDb db, Account account)
    {
      _db = db;
      Deposit = new Deposit(db) {Account = account};
      NewAccountForDeposit = null;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = Deposit.Account.Name;
      Deposit.CollectInfo();
      CanRenew = Deposit.State != DepositStates.Закрыт;
    }

    public void Renew()
    {
      var renewDepositViewModel = new RenewDepositViewModel(_db, Deposit);
      WindowManager.ShowDialog(renewDepositViewModel);
      if (renewDepositViewModel.NewDeposit != null)
      {
        NewAccountForDeposit = renewDepositViewModel.NewDeposit;
        Deposit.CollectInfo();
        CanRenew = Deposit.State != DepositStates.Закрыт;
        OnRenewed(NewAccountForDeposit);
      }
    }

    public void Exit()
    {
      TryClose();
    }

    public delegate void RenewedEventHandler(object sender, Account newAccount);
    public event RenewedEventHandler Renewed;
    protected void OnRenewed(Account newAccount)
    {
      if (Renewed != null) Renewed(this, newAccount);
    }



  }
}
