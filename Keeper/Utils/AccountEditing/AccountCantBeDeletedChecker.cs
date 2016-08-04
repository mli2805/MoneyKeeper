using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.AccountEditing
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

		    var hasRelatedTransactions = (from tran in _keeperDb.TransWithTags
		        where tran.MyAccount.Is(account) 
                      || (tran.MySecondAccount != null && tran.MySecondAccount.Is(account))
		              || (tran.Tags != null && tran.Tags.Contains(account))
		        select tran).Any();

			return hasRelatedTransactions ?
				       AccountCantBeDeletedReasons.HasRelatedTransactions 
				       : AccountCantBeDeletedReasons.CanDelete;
		}
	}
}