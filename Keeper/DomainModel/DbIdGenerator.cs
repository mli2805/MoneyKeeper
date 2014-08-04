using System.Composition;
using System.Linq;

using Keeper.Utils.Accounts;

namespace Keeper.DomainModel
{
	[Export]
	public sealed class DbIdGenerator
	{
		readonly KeeperDb _db;
		readonly AccountTreeStraightener _accountTreeStraightener;

		[ImportingConstructor]
		public DbIdGenerator(KeeperDb keeperDb, AccountTreeStraightener accountTreeStraightener)
		{
			_db = keeperDb;
			_accountTreeStraightener = accountTreeStraightener;
		}

		public int GenerateAccountId()
		{
			return (from account in _accountTreeStraightener.Flatten(_db.Accounts) 
			        select account.Id).Max() + 1;
		}

	    public int GenerateBankDepositOfferId()
	    {
	        if (_db.BankDepositOffers.Count == 0) return 1;
            return (from offer in _db.BankDepositOffers
                    select offer.Id).Max() + 1; 
	    }
	}
}