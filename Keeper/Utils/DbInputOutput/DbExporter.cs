using System.Composition;

using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	[Export]
	[Shared]
	internal class DbExporter
	{
		[Export]
		[Export(typeof(IKeeperDb))]
		public KeeperDb Db { get; private set; }
		[Export]
		public DbLoadResult LoadResult { get; private set; }

		[ImportingConstructor]
		public DbExporter(IDbGeneralLoader loader)
		{
			LoadResult = loader.LoadByExtension();
			Db = LoadResult.Db;
		}
	}
}