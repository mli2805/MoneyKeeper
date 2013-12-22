using System.Composition;
using System.IO;
using System.Windows;

using Keeper.Properties;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput
{
	[Export(typeof(IDbLocator))]
	public class DbLocator : IDbLocator
	{
		readonly IMessageBoxer mMessageBoxer;
		readonly IMyOpenFileDialog mOpenFileDialog;
		readonly IFileSystem mFileSystem;

		[ImportingConstructor]
		public DbLocator(IMessageBoxer messageBoxer, IMyOpenFileDialog openFileDialog, IFileSystem fileSystem)
		{
			mMessageBoxer = messageBoxer;
			mOpenFileDialog = openFileDialog;
			mFileSystem = fileSystem;
		}

		public string Locate()
		{
			var filename = mFileSystem.PathCombine(Settings.Default.DbPath, Settings.Default.DbxFile);
			if (!mFileSystem.GetFile(filename).Exists)
			{
				if (!AskUserForAnotherFile(ref filename))
					return null;
			}
			return filename;
		}

		private bool AskUserForAnotherFile(ref string filename)
		{
			if (mMessageBoxer.Show("File '" + filename +
			                       "' not found. \n\n You will be offered to choose database file. \n" +
			                       "\n Folder with chozen file will be accepted as new location for database " +
			                       "\n When exiting the program database and archive will be saved in this new folder" +
			                       "\n\n Would you like choose database or archive file?",
			                       "Error!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
				return false;

			filename = mOpenFileDialog.Show("*.*",
			                                "All files (*.*)|*.*|Keeper Database (.dbx)|*.dbx|Zip archive (with keeper database .zip)|*.zip|Text files (with data for keeper .txt)|*.txt",
			                                "");
			if (filename == "")
				return false;

			Settings.Default.DbPath = Path.GetDirectoryName(filename);
			return true;
		}

	}
}