using System.ComponentModel.Composition;
using System.Windows;

using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.FileSystem;

namespace Keeper.DbInputOutput
{
	[Export]
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class DbGeneralLoader
	{
		readonly IMessageBoxer mMessageBoxer;
		readonly IMyOpenFileDialog mOpenFileDialog;
		readonly IDbSerializer mDbSerializer;
		readonly IDbFromTxtLoader mFromTxtLoader;
		readonly IFileSystem mFileSystem;
		[Export]
		public KeeperDb Db { get; private set; }

		[ImportingConstructor]
		public DbGeneralLoader(IMessageBoxer messageBoxer, IMyOpenFileDialog openFileDialog, 
			IDbSerializer dbSerializer, IDbFromTxtLoader fromTxtLoader, IFileSystem fileSystem)
		{
			mMessageBoxer = messageBoxer;
			mOpenFileDialog = openFileDialog;
			mDbSerializer = dbSerializer;
			mFromTxtLoader = fromTxtLoader;
			mFileSystem = fileSystem;
			Db = FullDbLoadProcess();
		}

		KeeperDb FullDbLoadProcess()
		{
			var filename = mFileSystem.PathCombine(Settings.Default.SavePath, "Keeper.dbx");
			if (!mFileSystem.GetFile(filename).Exists)
			{
				mMessageBoxer.Show("File '" + filename + 
					"' not found. \n\n You will be offered to choose database file.",
					"Error!", MessageBoxButton.OK, MessageBoxImage.Warning);

				filename = mOpenFileDialog.Show(".dbx", 
					"Keeper Database (.dbx)|*.dbx", filename, @"g:\local_keeperDb\Keeper.dbx");
			}
			var db = mDbSerializer.DecryptAndDeserialize(filename);
			if (db != null) return db;

			mMessageBoxer.Show("File '" + filename + "' not found. \n Last zip will be used.",
				"Error!", MessageBoxButton.OK, MessageBoxImage.Error);

			var loadResult = mFromTxtLoader.LoadFromLastZip(ref db);
			if (loadResult.Code == 0) return db;

			mMessageBoxer.Show(loadResult.Explanation + ". \n Application will be closed!", 
				"Error!", MessageBoxButton.OK, MessageBoxImage.Error);
			return null;
		}


	}
}
