namespace Keeper.Utils.FileSystem
{
	public interface IFileSystem
	{
		IFile GetFile(string path);
		IDirectory GetDirectory(string path);
		IFile CreateTempFile();
		IFile CreateTempFile(string directory);
		string PathCombine(params string[] parts);
		bool IsPathRooted(string filePath);
	}

}