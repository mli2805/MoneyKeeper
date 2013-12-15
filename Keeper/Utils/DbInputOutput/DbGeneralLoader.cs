using System.Composition;
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
		readonly IMessageBoxer _mMessageBoxer;
		readonly IMyOpenFileDialog _mOpenFileDialog;
		readonly IDbSerializer _mDbSerializer;
		readonly IDbFromTxtLoader _mFromTxtLoader;
		readonly IFileSystem _mFileSystem;
		[Export]
		[Export(typeof(IKeeperDb))]
		public KeeperDb Db { get; private set; }

		[ImportingConstructor]
		public DbGeneralLoader(IMessageBoxer messageBoxer, IMyOpenFileDialog openFileDialog, 
			IDbSerializer dbSerializer, IDbFromTxtLoader fromTxtLoader, IFileSystem fileSystem)
		{
			_mMessageBoxer = messageBoxer;
			_mOpenFileDialog = openFileDialog;
			_mDbSerializer = dbSerializer;
			_mFromTxtLoader = fromTxtLoader;
			_mFileSystem = fileSystem;
			Db = FullDbLoadProcess();
		}

		KeeperDb FullDbLoadProcess()
		{
			var filename = _mFileSystem.PathCombine(Settings.Default.SavePath, "Keeper.dbx");
			if (!_mFileSystem.GetFile(filename).Exists)
			{
				_mMessageBoxer.Show("File '" + filename + 
					"' not found. \n\n You will be offered to choose database file.",
					"Error!", MessageBoxButton.OK, MessageBoxImage.Warning);

				filename = _mOpenFileDialog.Show(".dbx", 
					"Keeper Database (.dbx)|*.dbx", filename, @"g:\local_keeperDb\Keeper.dbx");
			}
			var db = _mDbSerializer.DecryptAndDeserialize(filename);
			if (db != null) return db;

			_mMessageBoxer.Show("File '" + filename + "' not found. \n Last zip will be used.",
				"Error!", MessageBoxButton.OK, MessageBoxImage.Error);

			var loadResult = _mFromTxtLoader.LoadFromLastZip(ref db);
			if (loadResult.Code == 0) return db;

			_mMessageBoxer.Show(loadResult.Explanation + ". \n Application will be closed!", 
				"Error!", MessageBoxButton.OK, MessageBoxImage.Error);
			return null;
		}


	}
}
