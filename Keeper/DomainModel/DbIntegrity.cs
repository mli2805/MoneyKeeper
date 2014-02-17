using System.Composition;
using System.Linq;

namespace Keeper.DomainModel
{
	[Export]
	public sealed class DbIntegrity
	{
		readonly KeeperDb mKeeperDb;

		[ImportingConstructor]
		public DbIntegrity(KeeperDb keeperDb)
		{
			mKeeperDb = keeperDb;
		}

		public DeleteReasons CanDelete(Account account)
		{
			if (account.Parent == null)
			{
				return DeleteReasons.IsRoot;
			}
			if (account.Children.Count > 0)
			{
				return DeleteReasons.HasChildren;
			}

			var hasRelatedTransactions = 
				(from transaction in mKeeperDb.Transactions
				 where transaction.Debet == account 
				       || transaction.Credit == account
				       || transaction.Article == account
				 select transaction).Any();	

			return hasRelatedTransactions ?
				       DeleteReasons.HasRelatedTransactions 
				       : DeleteReasons.CanDelete;
		}
	}
}