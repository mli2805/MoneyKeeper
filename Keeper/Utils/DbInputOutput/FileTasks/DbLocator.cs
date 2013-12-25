﻿using System.Composition;
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
		const string CHOOSE_ANOTHER_FILE = "File '{0}' not found. \n" +
				"\n You will be offered to choose database file. \n" +
				"\n Folder with chosen file will be accepted as new location for the database. " +
				"\n When exiting the program database and archive will be saved in this new folder.\n" +
				"\n Would you like to choose database or archive file?";

		const string FILTERS = "All files (*.*)|*.*|" +
			"Keeper Database (.dbx)|*.dbx|" +
			"Zip archive (with keeper database .zip)|*.zip|" +
			"Text files (with data for keeper .txt)|*.txt";

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
			if (mFileSystem.GetFile(filename).Exists) return filename;

			var answer = mMessageBoxer.Show(string.Format(CHOOSE_ANOTHER_FILE, filename), 
				"Error!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (answer == MessageBoxResult.No) return null;

			var another = mOpenFileDialog.Show("*.*", FILTERS, "");

			if (another == "") return null;

			Settings.Default.DbPath = Path.GetDirectoryName(another);
			Settings.Default.Save();
			return another;
		}
	}
}