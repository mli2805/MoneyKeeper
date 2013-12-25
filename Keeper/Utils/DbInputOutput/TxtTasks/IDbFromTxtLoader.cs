using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	interface IDbFromTxtLoader
	{
		DbLoadResult LoadDbFromTxt(string path);
	}
}