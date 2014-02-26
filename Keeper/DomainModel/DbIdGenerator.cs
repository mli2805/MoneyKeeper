using System.Composition;
using System.Linq;

using Keeper.Utils.Accounts;

namespace Keeper.DomainModel
{
	[Export]
	public sealed class DbIdGenerator
	{
		readonly KeeperDb mKeeperDb;
		readonly AccountTreeStraightener mAccountTreeStraightener;

		[ImportingConstructor]
		public DbIdGenerator(KeeperDb keeperDb, AccountTreeStraightener accountTreeStraightener)
		{
			mKeeperDb = keeperDb;
			mAccountTreeStraightener = accountTreeStraightener;
		}

		public int Generate()
		{
			return (from account in mAccountTreeStraightener.Flatten(mKeeperDb.Accounts) 
			        select account.Id).Max() + 1;
		}
	}
}