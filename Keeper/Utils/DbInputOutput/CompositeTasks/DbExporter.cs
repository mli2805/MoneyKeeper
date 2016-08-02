using System.Composition;
using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.DbInputOutput.CompositeTasks
{
	[Export]
	[Shared]
	internal class DbExporter
	{
		[Export]
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