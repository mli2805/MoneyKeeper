using System;
using System.Collections.Generic;
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
		readonly IFileExistenceChecker mExistenceChecker;

		[ImportingConstructor]
		public DbFromZipLoader(IDbFromTxtLoader dbFromTxtLoader, IFileSystem fileSystem, IFileExistenceChecker existenceChecker)
		{
			mDbFromTxtLoader = dbFromTxtLoader;
			mFileSystem = fileSystem;
			mExistenceChecker = existenceChecker;
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
				foreach (var innerFile in zipFile)
					if (!innerFile.ExtractWithPassword(unpackDirectory, ExtractExistingFileAction.OverwriteSilently, "!opa1526"))
						return new DbLoadError { Code = 21, Explanation = "Bad password!" };
			}
			return mExistenceChecker.Check(mFiles);
		}
	

		private readonly Dictionary<string, int> mFiles = new Dictionary<string, int>
			{
				{ "Accounts.txt", 215 },
				{ "ArticlesAssociations.txt", 225 },
				{ "CurrencyRates.txt", 235 },
				{ "Transactions.txt", 245 },
			};

	}
}