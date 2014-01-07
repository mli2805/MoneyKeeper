using System.Collections.Generic;
using System.Composition;
using System.Text;

using Keeper.Utils.Common;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [Export (typeof(IDbTxtFileWriter))]
	public class DbTxtFileWriter : IDbTxtFileWriter
  {
		readonly IFileSystem mFileSystem;
    private readonly IMySettings _mySettings;
    readonly Encoding mEncoding1251 = Encoding.GetEncoding(1251);

		[ImportingConstructor]
		public DbTxtFileWriter(IFileSystem fileSystem, IMySettings mySettings)
		{
		  mFileSystem = fileSystem;
		  _mySettings = mySettings;
		}

    public void WriteDbFile(string filename, IEnumerable<string> content)
    {
      var path = (string)_mySettings.GetSetting("TemporaryTxtDbPath");
			var directory = mFileSystem.GetDirectory(path);
			if (!directory.Exists) directory.Create();

			var fullPath = mFileSystem.PathCombine(path, filename);
      mFileSystem.GetFile(fullPath).WriteAllLines(content, mEncoding1251);
		}
	}
}