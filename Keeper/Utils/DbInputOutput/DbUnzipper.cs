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

	[Export(typeof(IDbUnzipper))]
	class DbUnzipper : IDbUnzipper
	{
		readonly IFileSystem mFileSystem;
		readonly IFileExistenceChecker mExistenceChecker;

		[ImportingConstructor]
		public DbUnzipper(IFileSystem fileSystem, IFileExistenceChecker existenceChecker)
		{
			mFileSystem = fileSystem;
			mExistenceChecker = existenceChecker;
		}

		public DbLoadResult UnzipArchive(string zipFilename)
		{
			using (var zipFile = mFileSystem.GetFile(zipFilename).ReadZip())
			{
				foreach (var innerFile in zipFile)
					if (!innerFile.ExtractWithPassword(Settings.Default.TemporaryTxtDbPath, ExtractExistingFileAction.OverwriteSilently, "!opa1526"))
						return new DbLoadResult(21, "Bad password!");
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