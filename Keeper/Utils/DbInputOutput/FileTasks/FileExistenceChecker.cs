using System.Collections.Generic;
using System.Composition;
using Keeper.Properties;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput.FileTasks
{
	[Export(typeof(IFileExistenceChecker))]
	public class FileExistenceChecker : IFileExistenceChecker
	{
		readonly IFileSystem mFileSystem;

		[ImportingConstructor]
		public FileExistenceChecker(IFileSystem fileSystem)
		{
			mFileSystem = fileSystem;
		}

		public DbLoadResult Check(Dictionary<string, int> files)
		{
			foreach (var pair in files)
				if (!mFileSystem.GetFile(mFileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, pair.Key)).Exists)
					return new DbLoadResult(pair.Value, pair.Key + " not found!");
			return null;
		}
	}
}