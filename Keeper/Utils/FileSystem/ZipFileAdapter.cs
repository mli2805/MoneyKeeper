using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Ionic.Zip;

namespace Keeper.Utils.FileSystem
{
	public class ZipFileAdapter : IZipFile
	{
		readonly ZipFile mZipFile;

		public ZipFileAdapter(ZipFile zipFile)
		{
			mZipFile = zipFile;
		}

		public IEnumerator<IZipEntry> GetEnumerator()
		{
			return mZipFile.Select(f => new ZipEntryAdapter(f)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			mZipFile.Dispose();
		}
	}
}