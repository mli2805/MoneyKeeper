using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	interface IDbFromTxtLoader 
  {
    DbLoadError LoadDbFromTxt(ref KeeperDb db, string path);
	}
}