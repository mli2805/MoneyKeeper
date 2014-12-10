using System.Composition;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ByFunctional.AccountEditing
{
	[Export(typeof(IAccountCantBeDeletedChecker))]
	public sealed class AccountCantBeDeletedChecker : IAccountCantBeDeletedChecker
	{
		readonly KeeperDb _keeperDb;

		[ImportingConstructor]
		public AccountCantBeDeletedChecker(KeeperDb keeperDb)
		{
			_keeperDb = keeperDb;
		}

		public AccountCantBeDeletedReasons Check(Account account)
		{
			if (account.Parent == null)
			{
				return AccountCantBeDeletedReasons.IsRoot;
			}
			if (account.Children.Count > 0)
			{
				return AccountCantBeDeletedReasons.HasChildren;
			}

			var hasRelatedTransactions = 
				(from transaction in _keeperDb.Transactions
				 where transaction.Debet == account 
				       || transaction.Credit == account
				       || transaction.Article == account
				 select transaction).Any();	

			return hasRelatedTransactions ?
				       AccountCantBeDeletedReasons.HasRelatedTransactions 
				       : AccountCantBeDeletedReasons.CanDelete;
		}
	}
}