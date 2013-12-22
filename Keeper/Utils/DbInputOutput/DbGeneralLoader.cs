using System.Composition;
using System.IO;
using System.Windows;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;

using System.Linq;

namespace Keeper.Utils.DbInputOutput
{
	[Export]
	[Shared]
	internal class DbGeneralLoader
	{
		[Export]
		[Export(typeof(IKeeperDb))]
		public KeeperDb Db { get; private set; }
		[Export]
		public DbLoadResult LoadResult { get; private set; }

		readonly IMessageBoxer _messageBoxer;
		readonly IMyOpenFileDialog _openFileDialog;
		readonly IDbSerializer _dbSerializer;
		readonly IDbUnzipper mUnzipper;
		readonly IDbFromTxtLoader _fromTxtLoader;
		readonly IFileSystem _fileSystem;


		[ImportingConstructor]
		public DbGeneralLoader(IMessageBoxer messageBoxer, IMyOpenFileDialog openFileDialog,
		  IDbSerializer dbSerializer, IDbFromTxtLoader fromTxtLoader, IFileSystem fileSystem, IDbUnzipper unzipper)
		{
			_messageBoxer = messageBoxer;
			_openFileDialog = openFileDialog;
			_dbSerializer = dbSerializer;
			_fromTxtLoader = fromTxtLoader;
			_fileSystem = fileSystem;
			mUnzipper = unzipper;

			Db = FullDbLoadProcess();
		}

		private KeeperDb FullDbLoadProcess()
		{
			var filename = _fileSystem.PathCombine(Settings.Default.DbPath, Settings.Default.DbxFile);
			if (!_fileSystem.GetFile(filename).Exists)
			{
				if (!AskUserForAnotherFile(ref filename))
					return null;
			}
			return LoadInAppropriateWay(filename);
		}

		private KeeperDb LoadInAppropriateWay(string filename)
		{
			var extension = Path.GetExtension(filename);

			/*   var loaders = new ILoader[0];

			   var selectedLoader = loaders.FirstOrDefault(loader => loader.SupportedExtension == extension);
			   if (selectedLoader == null) return null;
			   return selectedLoader.Load(filename);*/


			if (extension == ".dbx")
			{
				var db = _dbSerializer.DecryptAndDeserialize(filename);
				if (db == null) LoadResult = new DbLoadResult(0x11, "Problem with dbx file!");
				LoadResult = new DbLoadResult(db);
				return db;
			}
			if (extension == ".zip")
			{
				var db = new KeeperDb();

				LoadResult = mUnzipper.UnzipArchive(filename);
				if (LoadResult != null) return null;

				LoadResult = _fromTxtLoader.LoadDbFromTxt(ref db, Settings.Default.TemporaryTxtDbPath);
				if (LoadResult != null) return null;
				LoadResult = new DbLoadResult(db);
				return db;
			}
			if (extension == ".txt")
			{
				var db = new KeeperDb();
				LoadResult = _fromTxtLoader.LoadDbFromTxt(ref db, Path.GetDirectoryName(filename));
				if (LoadResult != null) return null;
				LoadResult = new DbLoadResult(db);
				return db;
			}
			return null;
		}

		private bool AskUserForAnotherFile(ref string filename)
		{
			if (_messageBoxer.Show("File '" + filename +
							   "' not found. \n\n You will be offered to choose database file. \n" +
							   "\n Folder with chozen file will be accepted as new location for database " +
							   "\n When exiting the program database and archive will be saved in this new folder" +
							   "\n\n Would you like choose database or archive file?",
							   "Error!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
			{
				LoadResult = new DbLoadResult(0x1, "User refused file selection request");
				return false;
			}

			filename = _openFileDialog.Show("*.*",
											"All files (*.*)|*.*|Keeper Database (.dbx)|*.dbx|Zip archive (with keeper database .zip)|*.zip|Text files (with data for keeper .txt)|*.txt",
											"");
			if (filename == "")
			{
				LoadResult = new DbLoadResult(0x15, "Standart dbx file not found. User refused selection");
				return false;
			}

			Settings.Default.DbPath = Path.GetDirectoryName(filename);
			return true;
		}
	}
	interface ILoader
	{
		string SupportedExtension { get; }
		KeeperDb Load(string filename);
	}
}
