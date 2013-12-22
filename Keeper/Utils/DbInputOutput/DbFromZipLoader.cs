using System;
using System.Composition;
using System.IO;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput
{

	[Export(typeof(IDbFromZipLoader))]
	class DbFromZipLoader : IDbFromZipLoader
	{
		readonly IDbFromTxtLoader mDbFromTxtLoader;
		readonly IFileSystem mFileSystem;

		[ImportingConstructor]
		public DbFromZipLoader(IDbFromTxtLoader dbFromTxtLoader, IFileSystem fileSystem)
		{
			mDbFromTxtLoader = dbFromTxtLoader;
			mFileSystem = fileSystem;
		}

		public DbLoadError LoadDbFromZip(ref KeeperDb db, string zipFile)
		{
			var result = UnzipArchive(zipFile);
			return result.Code != 0 ? result : mDbFromTxtLoader.LoadDbFromTxt(ref db, Settings.Default.TemporaryTxtDbPath);
		}

		private DbLoadError UnzipArchive(string zipFilename)
		{
			var unpackDirectory = Settings.Default.TemporaryTxtDbPath;
			var file = mFileSystem.GetFile(zipFilename);
			using (var zipFile = file.ReadZip())
			{
				// here, we extract every entry, but we could extract conditionally
				// based on entry name, size, date, checkbox status, etc.  
				foreach (var innerFile in zipFile)
				{
					if (!innerFile.ExtractWithPassword(unpackDirectory, ExtractExistingFileAction.OverwriteSilently, "!opa1526"))
						return new DbLoadError { Code = 21, Explanation = "Bad password!" };
				}
			}
			if (!mFileSystem.GetFile(mFileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, "Accounts.txt")).Exists)
				return new DbLoadError { Code = 215, Explanation = "Accounts.txt not found!" };
			if (!mFileSystem.GetFile(mFileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, "ArticlesAssociations.txt")).Exists)
				return new DbLoadError { Code = 225, Explanation = "ArticlesAssociations.txt not found!" };
			if (!mFileSystem.GetFile(mFileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, "CurrencyRates.txt")).Exists)
				return new DbLoadError { Code = 235, Explanation = "CurrencyRates.txt not found!" };
			if (!mFileSystem.GetFile(mFileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, "Transactions.txt")).Exists)
				return new DbLoadError { Code = 245, Explanation = "Transactions.txt not found!" };
			return new DbLoadError();
		}

	}
}