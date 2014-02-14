using System.Collections.ObjectModel;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
	[Export]
	public class DepositViewModel : Screen
	{
	  private readonly DepositReporter _depositReporter;
	  private bool _canRenew;

		public IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

		public Deposit Deposit { get; set; }
    public ObservableCollection<string> Report { get; set; }
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

		[ImportingConstructor]
		public DepositViewModel(DepositReporter depositReporter)
		{
		  _depositReporter = depositReporter;
		  NewAccountForDeposit = null;
		}

    public void SetAccount(Deposit deposit)
		{
      Deposit = deposit;
      Report = _depositReporter.BuildReport(deposit);
		}

		protected override void OnViewLoaded(object view)
		{
			DisplayName = Deposit.ParentAccount.Name;
			CanRenew = Deposit.Evaluations.State != DepositStates.Закрыт;
		}

		public void Renew()
		{
			var renewDepositViewModel = IoC.Get<RenewDepositViewModel>();
			renewDepositViewModel.SetOldDeposit(Deposit);
			WindowManager.ShowDialog(renewDepositViewModel);
			if (renewDepositViewModel.NewDeposit != null)
			{
				NewAccountForDeposit = renewDepositViewModel.NewDeposit;
				CanRenew = Deposit.Evaluations.State != DepositStates.Закрыт;
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
