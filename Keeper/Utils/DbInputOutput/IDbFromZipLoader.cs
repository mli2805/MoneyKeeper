using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
  internal interface IDbFromZipLoader
  {
    DbLoadError LoadDbFromZip(ref KeeperDb db, string filename);
  }
}