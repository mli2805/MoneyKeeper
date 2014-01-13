using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Keeper.Utils.FileSystem
{
	public sealed class DirectoryImpl : IDirectory, IEquatable<DirectoryImpl>
	{
		public DirectoryImpl(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			FullName = path;
		}

		public string FullName { get; private set; }
		public string Name { get { return Path.GetFileName(FullName); } }
		public IDirectory Parent
		{
			get
			{
				var directoryName = Path.GetDirectoryName(FullName);
				if (directoryName == null) return null;
				return new DirectoryImpl(directoryName);
			}
		}

		public bool Exists
		{
			get
			{
				FullName = Path.Combine(Environment.CurrentDirectory, FullName);
				return Directory.Exists(FullName);
			}
		}

		public void Clear()
		{
			Directory.Delete(FullName, true);
			Directory.CreateDirectory(FullName);
		}

		public void Create()
		{
			Directory.CreateDirectory(FullName);
		}

		public IEnumerable<IFile> GetFiles()
		{
			return GetFiles("*", true);
		}

		public IEnumerable<IFile> GetFiles(string searchPattern, bool includeSubDirectories)
		{
			return Directory.GetFiles(
				FullName, searchPattern, includeSubDirectories
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly).Select(x => (IFile)new FileImpl(x));

		}

		public bool HasFiles { get { { return !GetFiles().Any(); } } }

		public void MoveTo(IDirectory targetDirectory)
		{
			if (targetDirectory.Exists)
				Directory.Delete(targetDirectory.FullName);

			// Delete might not be finished (see: http://bit.ly/1dvFlKY)
			var waitCount = 0;
			const int maxWaitCount = 10;
			while (targetDirectory.Exists && waitCount++ < maxWaitCount)
			{
				Thread.Sleep(10);
			}

			Directory.Move(FullName, targetDirectory.FullName);
		}

		public IFile GetFile(string subPath)
		{
			return new FileImpl(Path.Combine(FullName, subPath));
		}
		#region ' Equality '

		public bool Equals(DirectoryImpl other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(FullName, other.FullName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is DirectoryImpl && Equals((DirectoryImpl) obj);
		}

		public override int GetHashCode()
		{
			return FullName.GetHashCode();
		}

		public static bool operator ==(DirectoryImpl left, DirectoryImpl right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DirectoryImpl left, DirectoryImpl right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}