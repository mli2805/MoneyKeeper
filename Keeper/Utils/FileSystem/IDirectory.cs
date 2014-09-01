using System.Collections.Generic;

namespace Keeper.Utils.FileSystem
{
	public interface IDirectory
	{
		bool Exists { get; }
		bool HasFiles { get; }

		string FullName { get; }
		string Name { get; }
		IDirectory Parent { get; }

		void Clear();
		void Create();
		IEnumerable<IFile> GetFiles();
		IEnumerable<IFile> GetFiles(string searchPattern, bool includeSubDirectories);
		void MoveTo(IDirectory targetDirectory);
		IFile GetFile(string subPath);
	}
}