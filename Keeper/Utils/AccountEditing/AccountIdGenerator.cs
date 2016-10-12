using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;

namespace Keeper.Utils.AccountEditing
{
	[Export]
	public sealed class AccountIdGenerator
	{
		readonly KeeperDb _db;

		[ImportingConstructor]
		public AccountIdGenerator(KeeperDb keeperDb)
		{
			_db = keeperDb;
		}

		public int GenerateAccountId()
		{
			return (from account in _db.FlattenAccounts() 
			        select account.Id).Max() + 1;
		}

	}
}