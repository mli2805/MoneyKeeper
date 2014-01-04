using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Text;

using Keeper.Properties;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [Export (typeof(IDbTxtFileWriter))]
	public class DbTxtFileWriter : IDbTxtFileWriter
  {
		readonly IFileSystem mFileSystem;
		readonly Encoding mEncoding1251 = Encoding.GetEncoding(1251);

		[ImportingConstructor]
		public DbTxtFileWriter(IFileSystem fileSystem)
		{
			mFileSystem = fileSystem;
		}

		public void WriteDbFile(string accountsTxt, IEnumerable<string> accountsToList)
		{
			var directory = mFileSystem.GetDirectory(Settings.Default.TemporaryTxtDbPath);
			if (!directory.Exists) directory.Create();

			var fullPath = Path.Combine(Settings.Default.TemporaryTxtDbPath, accountsTxt);
			File.WriteAllLines(fullPath, accountsToList, mEncoding1251);
		}
	}
}