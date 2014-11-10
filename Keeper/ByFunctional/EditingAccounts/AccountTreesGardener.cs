using System.Composition;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Accounts;

namespace Keeper.ByFunctional.EditingAccounts
{
	[Export]
	public class AccountTreesGardener
	{
		readonly IAccountFactory _accountFactory;
		readonly IUserInformator _userInformator;
		readonly IAccountOperator _accountOperator;
		readonly IAccountCantBeDeletedChecker _accountCantBeDeletedChecker;
		readonly IUserEquirer _userEquirer;

		[ImportingConstructor]
		public AccountTreesGardener(IAccountFactory accountFactory, IUserInformator userInformator,
			IAccountOperator accountOperator, IAccountCantBeDeletedChecker accountCantBeDeletedChecker, IUserEquirer userEquirer)
		{
			_accountFactory = accountFactory;
			_userInformator = userInformator;
			_accountOperator = accountOperator;
			_accountCantBeDeletedChecker = accountCantBeDeletedChecker;
			_userEquirer = userEquirer;
		}

		public void RemoveAccount(Account selectedAccount)
		{
			switch (_accountCantBeDeletedChecker.Check(selectedAccount))
			{
				case AccountCantBeDeletedReasons.CanDelete:
					if (_userEquirer.ToDeleteAccount(selectedAccount))
						_accountOperator.RemoveNode(selectedAccount);
					break;
				case AccountCantBeDeletedReasons.IsRoot:
					_userInformator.YouCannotRemoveRootAccount();
					break;
				case AccountCantBeDeletedReasons.HasChildren:
					_userInformator.YouCannotRemoveAccountWithChildren();
					break;
				case AccountCantBeDeletedReasons.HasRelatedTransactions:
					_userInformator.YouCannotRemoveAccountThatHasRelatedTransactions();
					break;
			}
		}

		public Account AddAccount(Account selectedAccount)
		{
			var accountInWork = _accountFactory.CreateAccount(selectedAccount);
            accountInWork.IsClosed = selectedAccount.Is("Закрытые");
            return !_userEquirer.ToAddAccount(accountInWork) ? null : _accountOperator.AddNode(accountInWork);
		}

		public void ChangeAccount(Account selectedAccount)
		{
			var accountInWork = _accountFactory.CloneAccount(selectedAccount);
			if (!_userEquirer.ToEditAccount(accountInWork)) return;
			_accountOperator.ApplyEdit(ref selectedAccount, accountInWork);
		}

        public Account AddDeposit(Account selectedAccount)
        {
            var accountInWork = _accountFactory.CreateAccount(selectedAccount);
            accountInWork.IsClosed = selectedAccount.Is("Закрытые депозиты");
            var depositInWork = new Deposit { ParentAccount = accountInWork };

            if (!_userEquirer.ToAddDeposit(depositInWork)) return null;
            accountInWork.Deposit = depositInWork;
            return _accountOperator.AddNode(accountInWork);
        }

        public void ChangeDeposit(Account selectedAccount)
        {
            var accountInWork = _accountFactory.CloneAccount(selectedAccount);
            var depositInWork = accountInWork.Deposit;
            if (!_userEquirer.ToEditDeposit(depositInWork)) return;
            _accountOperator.ApplyEdit(ref selectedAccount, accountInWork);
        }

	}
}
