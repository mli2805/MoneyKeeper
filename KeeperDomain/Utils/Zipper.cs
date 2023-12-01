using System;
using System.IO;
using System.IO.Compression;

namespace KeeperDomain
{
    public static class Zipper
    {
        public static LibResult ZipTxtFiles()
        {
            var archiveName = $"DB{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
            var zipFileToCreate = Path.Combine(PathFactory.GetBackupFilePath(archiveName));
            var backupFolder = PathFactory.GetBackupPath();
            try
            {
                var zip = ZipFile.Open(zipFileToCreate, ZipArchiveMode.Create);
                var filenames = Directory.GetFiles(backupFolder, "*.txt"); // note: this does not recurse directories! 
                foreach (var filename in filenames)
                {
                    zip.CreateEntryFromFile(filename, Path.GetFileName(filename), CompressionLevel.Optimal);
                }
                zip.Dispose();
                return new LibResult(true, null);
            }
            catch (Exception e)
            {
                return new LibResult(e, "ZipTxtFiles");
            }
        }
    }
}
