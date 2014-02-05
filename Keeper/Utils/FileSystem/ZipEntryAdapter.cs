using System;

using Ionic.Zip;

namespace Keeper.Utils.FileSystem
{
	public class ZipEntryAdapter : IZipEntry
	{
		readonly ZipEntry mZipEntry;

		public ZipEntryAdapter(ZipEntry zipEntry)
		{
			mZipEntry = zipEntry;
		}

		public bool ExtractWithPassword(string unpackDirectory, ExtractExistingFileAction extractExistingFileAction, string password)
		{
			try
			{
				mZipEntry.ExtractWithPassword(unpackDirectory, extractExistingFileAction, password);
			}
			catch (Exception exception)
			{
				if (exception is BadPasswordException) return false;
				throw;
			}
			return true;
		}


	}
}