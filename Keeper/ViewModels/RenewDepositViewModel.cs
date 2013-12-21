using System;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;

namespace Keeper.ViewModels
{
	[Export]
	class RenewDepositViewModel : Screen
	{
	  private readonly KeeperDb _db;
    private readonly BalanceCalculator _balanceCalculator;
	  private readonly AccountInTreeSeeker _accountInTreeSeeker;

		private Deposit _oldDeposit;
		public Account NewDeposit { get; set; }

		public DateTime TransactionsDate { get; set; }
		public string OldDepositName { get; set; }
		public string DepositCurrency { get; set; }
		public Account BankAccount { get; set; }
		public decimal Procents { get; set; }
		public string NewDepositName { get; set; }

		[ImportingConstructor]
		public RenewDepositViewModel(KeeperDb db, BalanceCalculator balanceCalculator, AccountInTreeSeeker accountInTreeSeeker)
		{
		  _db = db;
		  _balanceCalculator = balanceCalculator;
		  _accountInTreeSeeker = accountInTreeSeeker;
		}

		public void SetOldDeposit(Deposit oldDeposit)
		{
			_oldDeposit = oldDeposit;

			TransactionsDate = _oldDeposit.Finish;
			OldDepositName = _oldDeposit.Account.Name;
			DepositCurrency = _oldDeposit.MainCurrency.ToString().ToLower();
			BankAccount = FindBankAccount();
			Procents = _oldDeposit.Forecast;
			NewDepositName = BuildNewName();
		}

		protected override void OnViewLoaded(object view)
		{
			DisplayName = "Переоформление";
		}

		private Account FindBankAccount()
		{
			var st = OldDepositName.Substring(0, OldDepositName.IndexOf(' '));
			return _accountInTreeSeeker.FindAccountInTree(st);
		}

		private string BuildNewName()
		{
			var st = OldDepositName.Substring(0, OldDepositName.IndexOf('/') - 2).Trim();
			DateTime newFinish = _oldDeposit.Finish + (_oldDeposit.Finish - _oldDeposit.Start);
			string period = String.Format("{0:d/MM/yyyy} - {1:d/MM/yyyy}", _oldDeposit.Finish, newFinish).Replace('.', '/');
			return String.Format("{0} {1} {2}%", st, period, _oldDeposit.DepositRate);
		}

		private Account AddNewAccountForDeposit()
		{
			var newDepositAccount = new Account(NewDepositName);
			newDepositAccount.Id = (from account in _db.AccountsPlaneList select account.Id).Max() + 1;

			var parent = _accountInTreeSeeker.FindAccountInTree("Депозиты");
			newDepositAccount.Parent = parent;
			parent.Children.Add(newDepositAccount);

			_db.AccountsPlaneList.Clear();
			_db.AccountsPlaneList = new AccountTreeStraightener().FillInAccountsPlaneList(_db.Accounts);
			UsefulLists.FillLists(_db);

			return newDepositAccount;
		}

		private DateTime GetTimestampForTransactions()
		{
			var lastTransactionInDay =
			   (from t in _db.Transactions where t.Timestamp.Date == TransactionsDate.Date select t).LastOrDefault();
			return lastTransactionInDay == null ?
			  TransactionsDate.AddHours(9) :
			  lastTransactionInDay.Timestamp.AddMinutes(1);
		}

		private void MakeTransactionProcents()
		{
			var transactionProcents = new Transaction
			  {
				  Timestamp = GetTimestampForTransactions(),
				  Operation = OperationType.Доход,
				  Debet = BankAccount,
				  Credit = _oldDeposit.Account,
				  Amount = Procents,
				  Currency = _oldDeposit.MainCurrency,
				  Article = _accountInTreeSeeker.FindAccountInTree("Проценты по депозитам"),
				  Comment = "причисление процентов при закрытии"
			  };

			_db.Transactions.Add(transactionProcents);
		}

		private void MakeTransactionTransfer()
		{
			var transactionProcents = new Transaction
			{
				Timestamp = GetTimestampForTransactions(),
				Operation = OperationType.Перенос,
				Debet = _oldDeposit.Account,
				Credit = NewDeposit,
				Amount = _balanceCalculator.GetBalanceInCurrency(_oldDeposit.Account,
                            new Period(new DateTime(0), GetTimestampForTransactions()),
													  _oldDeposit.MainCurrency),
				Currency = _oldDeposit.MainCurrency,
				Comment = "переоформление вклада"
			};

			_db.Transactions.Add(transactionProcents);
		}

		private void RemoveOldAccountToClosed()
		{
			var parent = _accountInTreeSeeker.FindAccountInTree("Депозиты");
			parent.Children.Remove(_oldDeposit.Account);

			parent = _accountInTreeSeeker.FindAccountInTree("Закрытые депозиты");
			_oldDeposit.Account.Parent = parent;
			parent.Children.Add(_oldDeposit.Account);
		}

		public void Accept()
		{
			MakeTransactionProcents();
			NewDeposit = AddNewAccountForDeposit();
			MakeTransactionTransfer();
			RemoveOldAccountToClosed();
			TryClose();
		}

		public void Decline()
		{
		}

	}


}
