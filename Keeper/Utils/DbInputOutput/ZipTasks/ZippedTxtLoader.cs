using System.Composition;

using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput
{
	[Export(typeof(ILoader))]
	class ZippedTxtLoader : ILoader
	{
		readonly IDbUnzipper mUnzipper;
		readonly IDbFromTxtLoader mTxtLoader;
		[ImportingConstructor]
		public ZippedTxtLoader(IDbUnzipper unzipper, IDbFromTxtLoader txtLoader)
		{
			mUnzipper = unzipper;
			mTxtLoader = txtLoader;
		}

		public string FileExtension { get { return ".zip"; } }
		public DbLoadResult Load(string filename)
		{
			var loadResult = mUnzipper.UnzipArchive(filename);
			if (loadResult != null) return loadResult;

			return mTxtLoader.LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
		}
	}
}