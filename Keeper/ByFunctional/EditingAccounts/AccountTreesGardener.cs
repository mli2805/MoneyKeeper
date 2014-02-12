using System.Composition;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.ByFunctional.EditingAccounts
{
	[Export]
	public class AccountTreesGardener
	{
		readonly IMyFactory _myFactory;
		readonly TellUser _tellUser;
		readonly AccountOperations _accountOperations;
		readonly DbIntegrity _dbIntegrity;
		readonly AskUser _askUser;

		[ImportingConstructor]
		public AccountTreesGardener(IMyFactory myFactory, TellUser tellUser,
			AccountOperations accountOperations, DbIntegrity dbIntegrity, AskUser askUser)
		{
			_myFactory = myFactory;
			_tellUser = tellUser;
			_accountOperations = accountOperations;
			_dbIntegrity = dbIntegrity;
			_askUser = askUser;
		}

		public void RemoveAccount(Account selectedAccount)
		{
			switch (_dbIntegrity.CanDelete(selectedAccount))
			{
				case DeleteReasons.CanDelete:
					if (_askUser.ToDeleteAccount(selectedAccount))
						_accountOperations.RemoveNode(selectedAccount);
					break;
				case DeleteReasons.IsRoot:
					_tellUser.YouCannotRemoveRootAccount();
					break;
				case DeleteReasons.HasChildren:
					_tellUser.YouCannotRemoveAccountWithChildren();
					break;
				case DeleteReasons.HasRelatedTransactions:
					_tellUser.YouCannotRemoveAccountThatHasRelatedTransactions();
					break;
			}
		}

		public Account AddAccount(Account selectedAccount)
		{
			var accountInWork = _myFactory.CreateAccount(selectedAccount);
			if (!_askUser.ToAddAccount(accountInWork)) return null;
			return _accountOperations.AddNode(accountInWork);
		}

		public void ChangeAccount(Account selectedAccount)
		{
			var accountInWork = _myFactory.CloneAccount(selectedAccount);
			if (!_askUser.ToEditAccount(accountInWork)) return;
			_accountOperations.ApplyEdit(selectedAccount, accountInWork);
		}

    public Account AddDeposit(Account selectedAccount)
    {
      var accountInWork = _myFactory.CreateAccount(selectedAccount);
      var depositInWork = new Deposit {Account = accountInWork};

      if (!_askUser.ToAddDeposit(depositInWork)) return null;
      // сохранить depositInWork

      //
      return _accountOperations.AddNode(accountInWork);
    }


	}
}
