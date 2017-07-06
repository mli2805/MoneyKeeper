using System;
using System.Composition;
using System.IO;
using System.Linq;

namespace Keeper.Utils.FileSystem
{
	[Export(typeof(IFileSystem))]
	[Shared]
	public sealed class FileSystemImpl : IFileSystem
	{
		public IFile GetFile(string path)
		{
			return new FileImpl(path);
		}

		public IDirectory GetDirectory(string path)
		{
			return new DirectoryImpl(path);
		}

		public IFile CreateTempFile()
		{
			return new FileImpl(Path.GetTempFileName());
		}

		public IFile CreateTempFile(string directory)
		{
			var path = Path.Combine(directory, Guid.NewGuid().ToString());
			File.WriteAllText(path, "");
			return new FileImpl(path);
		}

		public string PathCombine(params string[] parts)
		{
			var result = parts[0];
			// ReSharper disable LoopCanBeConvertedToQuery because of AoT
			foreach (var part in parts.Skip(1))
			{
				result = Path.Combine(result, part);
			}
			// ReSharper restore LoopCanBeConvertedToQuery
			return result;
		}

		public bool IsPathRooted(string filePath)
		{
			return Path.IsPathRooted(filePath);
		}
	}
}