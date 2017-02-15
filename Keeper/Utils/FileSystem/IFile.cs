using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Keeper.Utils.FileSystem
{
	public interface IFile
	{
		bool Exists { get; }
		IDirectory Parent { get; }
		string Name { get; }
		string FullName { get; }
		long Size { get; }

		TextWriter AppendText();
		void CopyOrReplace(string destinationFileName);
		void Delete();
		void MoveTo(string destination);
		XmlReader OpenXml();
		Stream OpenRead();
		TextReader OpenText();
		TextReader OpenText(Encoding encoding);
		Stream OpenWrite();
		/// <summary>See <see cref="FileImpl"/>.<see cref="File.Replace(string,string,string,bool)"/></summary>
		void Replace(string destinationFileName, string destinationBackupFileName = null, bool ignoreMetadataErrors = true);

		string RelativeTo(IDirectory directory);
		IFile ChangeExtension(string extension);
		IZipFile ReadZip();

    void WriteAllLines(IEnumerable<string> content, Encoding encoding);
	}
}