using Ionic.Zip;

namespace Keeper.Utils.FileSystem
{
	public interface IZipEntry {
		/// <returns>false if password is wrong</returns>
		bool ExtractWithPassword(string unpackDirectory, ExtractExistingFileAction extractExistingFileAction, string password);
	}
}