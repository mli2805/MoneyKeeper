using System.Composition;

using Keeper.DomainModel;

namespace Keeper.Utils.Accounts
{
	[Export]
	public class AccountTreesGardener
	{
		readonly IMyFactory mMyFactory;
		readonly TellUser mTellUser;
		readonly AccountOperations mAccountOperations;
		readonly DbIntegrity mDbIntegrity;
		readonly AskUser mAskUser;

		[ImportingConstructor]
		public AccountTreesGardener(IMyFactory myFactory, TellUser tellUser,
			AccountOperations accountOperations, DbIntegrity dbIntegrity, AskUser askUser)
		{
			mMyFactory = myFactory;
			mTellUser = tellUser;
			mAccountOperations = accountOperations;
			mDbIntegrity = dbIntegrity;
			mAskUser = askUser;
		}

		public void RemoveAccount(Account selectedAccount)
		{
			switch (mDbIntegrity.CanDelete(selectedAccount))
			{
				case DeleteReasons.CanDelete:
					if (mAskUser.ToDeleteAccount(selectedAccount))
						mAccountOperations.RemoveNode(selectedAccount);
					break;
				case DeleteReasons.IsRoot:
					mTellUser.YouCannotRemoveRootAccount();
					break;
				case DeleteReasons.HasChildren:
					mTellUser.YouCannotRemoveAccountWithChildren();
					break;
				case DeleteReasons.HasRelatedTransactions:
					mTellUser.YouCannotRemoveAccountThatHasRelatedTransactions();
					break;
			}
		}

		public Account AddAccount(Account selectedAccount)
		{
			var accountInWork = mMyFactory.CreateAccount(selectedAccount);
			if (mAskUser.ToAddAccount(accountInWork)) return null;
			return mAccountOperations.AddNode(accountInWork);
		}

		public void ChangeAccount(Account selectedAccount)
		{
			var accountInWork = mMyFactory.CloneAccount(selectedAccount);
			if (mAskUser.ToEditAccount(accountInWork)) return;
			mAccountOperations.ApplyEdit(selectedAccount, accountInWork);
		}
	}
}
