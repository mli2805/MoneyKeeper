using System.Composition;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ByFunctional.AccountEditing
{
	[Export]
	public sealed class AccountIdGenerator
	{
		readonly KeeperDb _db;
		readonly AccountTreeStraightener _accountTreeStraightener;

		[ImportingConstructor]
		public AccountIdGenerator(KeeperDb keeperDb, AccountTreeStraightener accountTreeStraightener)
		{
			_db = keeperDb;
			_accountTreeStraightener = accountTreeStraightener;
		}

		public int GenerateAccountId()
		{
			return (from account in _accountTreeStraightener.Flatten(_db.Accounts) 
			        select account.Id).Max() + 1;
		}

	}
}