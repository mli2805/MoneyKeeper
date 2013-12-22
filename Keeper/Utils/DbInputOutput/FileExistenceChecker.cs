using System.Collections.Generic;
using System.Composition;

using Keeper.Properties;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput
{
	[Export (typeof(IFileExistenceChecker))]
	public class FileExistenceChecker : IFileExistenceChecker
	{
		readonly IFileSystem mFileSystem;

		[ImportingConstructor]
		public FileExistenceChecker(IFileSystem fileSystem)
		{
			mFileSystem = fileSystem;
		}

		public DbLoadError Check(Dictionary<string, int> files)
		{
			foreach (var pair in files)
				if (!mFileSystem.GetFile(mFileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, pair.Key)).Exists)
					return new DbLoadError { Code = pair.Value, Explanation = pair.Key + " not found!" };
			return new DbLoadError();
		}
	}
}