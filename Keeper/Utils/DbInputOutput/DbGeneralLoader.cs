using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;

namespace Keeper.Utils.DbInputOutput
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
			var extension = Path.GetExtension(filename);
			var selectedLoader = mLoaders.Single(loader => loader.SupportedExtension == extension);
			return selectedLoader.Load(filename);
		}

	}
}
