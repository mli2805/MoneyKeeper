using System.Composition;
using System.IO;
using System.Windows;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput
{
	[Export]
	[Shared]
	internal class DbGeneralLoader
	{
		readonly IMessageBoxer _messageBoxer;
		readonly IMyOpenFileDialog _openFileDialog;
		readonly IDbSerializer _dbSerializer;
		readonly IDbFromTxtLoader _fromTxtLoader;
		readonly IFileSystem _fileSystem;
		[Export]
		[Export(typeof(IKeeperDb))]
		public KeeperDb Db { get; private set; }

		[ImportingConstructor]
		public DbGeneralLoader(IMessageBoxer messageBoxer, IMyOpenFileDialog openFileDialog, 
			IDbSerializer dbSerializer, IDbFromTxtLoader fromTxtLoader, IFileSystem fileSystem)
		{
			_messageBoxer = messageBoxer;
			_openFileDialog = openFileDialog;
			_dbSerializer = dbSerializer;
			_fromTxtLoader = fromTxtLoader;
			_fileSystem = fileSystem;
			Db = FullDbLoadProcess();
		}

		KeeperDb FullDbLoadProcess()
		{
      var filename = Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile);
			if (!_fileSystem.GetFile(filename).Exists)
			{
				_messageBoxer.Show("File '" + filename + 
					"' not found. \n\n You will be offered to choose database file.",
					"Error!", MessageBoxButton.OK, MessageBoxImage.Warning);

			  var startupPath = Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
			  var testBasePath = Path.GetFullPath(Path.Combine(startupPath, Settings.Default.TestDbPath));

			  filename = _openFileDialog.Show(".dbx", "Keeper Database (.dbx)|*.dbx", testBasePath);
        if (filename == "") return null;

        Settings.Default.DbPath = Path.GetDirectoryName(filename);
			  Settings.Default.DbxFile = filename;
			}

			var db = _dbSerializer.DecryptAndDeserialize(filename);
			if (db != null) return db;

			_messageBoxer.Show("File '" + filename + "' not found. \n Last zip will be used.",
				"Error!", MessageBoxButton.OK, MessageBoxImage.Error);

			var loadResult = _fromTxtLoader.LoadFromLastZip(ref db);
			if (loadResult.Code == 0) return db;

			_messageBoxer.Show(loadResult.Explanation + ". \n Application will be closed!", 
				"Error!", MessageBoxButton.OK, MessageBoxImage.Error);
			return null;
		}


	}
}
