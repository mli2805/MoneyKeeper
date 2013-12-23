using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using Ionic.Zip;

using System.Linq;

using Keeper.Utils.DbInputOutput;

namespace Keeper.Utils.FileSystem
{
	public sealed class FileImpl : IFile
	{
		public FileImpl(string filePath)
		{
			FullName = filePath;
		}

		public XmlReader OpenXml()
		{
			return new XmlTextReader(FullName);
		}

		public Stream OpenRead()
		{
			return File.OpenRead(FullName);
		}

		public TextReader OpenText(Encoding encoding)
		{
			if (FullName == null)
				throw new ArgumentNullException("path");
			return new StreamReader(FullName, encoding);
		}

		public Stream OpenWrite()
		{
			return File.OpenWrite(FullName);
		}

		public void Replace(string destinationFileName, string destinationBackupFileName = null, bool ignoreMetadataErrors = true)
		{
			File.Replace(FullName, destinationFileName, destinationBackupFileName);
		}

		public string RelativeTo(IDirectory directory)
		{
			var obj = FullName.Replace('\\', '/');
			var subj = directory.FullName.Replace('\\', '/');
			if (!obj.StartsWith(subj)) throw new ArgumentException("File is not relative to directory");
			return obj.Replace(subj, "").TrimStart('/');
		}

		public IFile ChangeExtension(string extension)
		{
			return new FileImpl(Path.ChangeExtension(FullName, extension));
		}

		public IZipFile ReadZip()
		{
			return new ZipFileAdapter(ZipFile.Read(FullName));
		}

		public TextReader OpenText()
		{
			return File.OpenText(FullName);
		}

		public bool Exists { get { return File.Exists(FullName); } }
		public IDirectory Parent
		{
			get
			{
				var directoryName = Path.GetDirectoryName(FullName);
				if (directoryName == null) throw new DirectoryNotFoundException(
					"FileImpl is initialized with invalid full-name of the file, " +
					"because an attempt to get its directory name failed.");
				return new DirectoryImpl(directoryName);
			}
		}
		public string Name { get { return Path.GetFileName(FullName); } }
		public string FullName { get; private set; }
		public long Size { get { return new FileInfo(FullName).Length; } }

		public TextWriter AppendText()
		{
			return File.AppendText(FullName);
		}

		public void CopyOrReplace(string destinationFileName)
		{
			if (!File.Exists(destinationFileName))
				File.Move(FullName, destinationFileName);
			else
				/* System.IO.FileInfo.Replace will also throw the above exception when 
				 * the destination volume differs from the source file’s volume. http://bit.ly/154xmnm */
				File.Replace(FullName, destinationFileName, null, true);
		}

		public void Delete()
		{
			File.Delete(FullName);
		}

		public void MoveTo(string destination)
		{
			File.Move(FullName, destination);
		}
	}
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