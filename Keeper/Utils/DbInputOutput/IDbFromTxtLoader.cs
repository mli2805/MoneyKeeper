using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	interface IDbFromTxtLoader
	{
		DbLoadResult LoadDbFromTxt(ref KeeperDb db, string path);
	}
}