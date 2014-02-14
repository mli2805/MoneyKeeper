using System;
using System.Collections.Generic;
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
		readonly AccountTreeStraightener _accountTreeStraightener;
    private DepositEvaluations _oldDepositEvaluations;
		public Account NewDeposit { get; set; }

		public DateTime TransactionsDate { get; set; }
		public string OldDepositName { get; set; }
		public string DepositCurrency { get; set; }
		public Account BankAccount { get; set; }
		public decimal Procents { get; set; }
		public string NewDepositName { get; set; }

    public List<Account> BankAccounts { get; set; }

		[ImportingConstructor]
		public RenewDepositViewModel(KeeperDb db, BalanceCalculator balanceCalculator, 
			 AccountTreeStraightener accountTreeStraightener)
		{
		  _db = db;
		  _balanceCalculator = balanceCalculator;
      _accountTreeStraightener = accountTreeStraightener;

      BankAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
		}

		public void SetOldDeposit(DepositEvaluations oldDepositEvaluations)
		{
      _oldDepositEvaluations = oldDepositEvaluations;

      TransactionsDate = _oldDepositEvaluations.DepositCore.FinishDate;
      OldDepositName = _oldDepositEvaluations.DepositCore.ParentAccount.Name;
      DepositCurrency = _oldDepositEvaluations.DepositCore.Currency.ToString().ToLower();
			BankAccount = FindBankAccount();
      Procents = _oldDepositEvaluations.EstimatedProcents;
			NewDepositName = BuildNewName();
		}

		protected override void OnViewLoaded(object view)
		{
      BankAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
      DisplayName = "Переоформление";
		}

		private Account FindBankAccount()
		{
			var st = OldDepositName.Substring(0, OldDepositName.IndexOf(' '));
			return _accountTreeStraightener.Seek(st,_db.Accounts);
		}

		private string BuildNewName()
		{
			var st = OldDepositName.Substring(0, OldDepositName.IndexOf('/') - 2).Trim();
      DateTime newFinish = _oldDepositEvaluations.DepositCore.FinishDate + (_oldDepositEvaluations.DepositCore.FinishDate - _oldDepositEvaluations.DepositCore.StartDate);
      string period = String.Format("{0:d/MM/yyyy} - {1:d/MM/yyyy}", _oldDepositEvaluations.DepositCore.FinishDate, newFinish).Replace('.', '/');
      return String.Format("{0} {1} {2}%", st, period, _oldDepositEvaluations.DepositCore.DepositRate);
		}

		private Account AddNewAccountForDeposit()
		{
			var newDepositAccount = new Account(NewDepositName);
			newDepositAccount.Id = (from account in new AccountTreeStraightener().Flatten(_db.Accounts) select account.Id).Max() + 1;

			var parent = _accountTreeStraightener.Seek("Депозиты", _db.Accounts);
			newDepositAccount.Parent = parent;
			parent.Children.Add(newDepositAccount);

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
          Credit = _oldDepositEvaluations.DepositCore.ParentAccount,
				  Amount = Procents,
          Currency = _oldDepositEvaluations.DepositCore.Currency,
				  Article = _accountTreeStraightener.Seek("Проценты по депозитам", _db.Accounts),
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
        Debet = _oldDepositEvaluations.DepositCore.ParentAccount,
				Credit = NewDeposit,
        Amount = _balanceCalculator.GetBalanceInCurrency(_oldDepositEvaluations.DepositCore.ParentAccount,
                            new Period(new DateTime(0), GetTimestampForTransactions()),
                            _oldDepositEvaluations.DepositCore.Currency),
        Currency = _oldDepositEvaluations.DepositCore.Currency,
				Comment = "переоформление вклада"
			};

			_db.Transactions.Add(transactionProcents);
		}

		private void RemoveOldAccountToClosed()
		{
			var parent = _accountTreeStraightener.Seek("Депозиты", _db.Accounts);
      parent.Children.Remove(_oldDepositEvaluations.DepositCore.ParentAccount);

			parent = _accountTreeStraightener.Seek("Закрытые депозиты", _db.Accounts);
      _oldDepositEvaluations.DepositCore.ParentAccount.Parent = parent;
      parent.Children.Add(_oldDepositEvaluations.DepositCore.ParentAccount);
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
