using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;

using Keeper.Utils.DbInputOutput.FileTasks;

namespace Keeper.Utils.DbInputOutput.CompositeTasks
{
	[Export(typeof(IDbGeneralLoader))]
	internal class DbGeneralLoader : IDbGeneralLoader
	{
		readonly IDbLocator mLocator;
		readonly IEnumerable<ILoader> mLoaders;

		[ImportingConstructor]
		public DbGeneralLoader(IDbLocator locator, [ImportMany] IEnumerable<ILoader> loaders)
		{
			mLocator = locator;
			mLoaders = loaders;
		}

		public DbLoadResult LoadByExtension()
		{
			var filename = mLocator.Locate();
			if (filename == null) return new DbLoadResult(52354, "User refused to choose another file");
			var extension = Path.GetExtension(filename);
			var selectedLoader = mLoaders.FirstOrDefault(loader => loader.FileExtension == extension);
			if (selectedLoader == null) return new DbLoadResult(52355, "User has chosen file with wrong extension");
			return selectedLoader.Load(filename);
		}

	}
}
