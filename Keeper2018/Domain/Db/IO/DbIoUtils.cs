using System;
using System.IO;
using System.Linq;

namespace Keeper2018
{
    public static class DbIoUtils
    {
        // export from old Keeper
        public static string GetOldTxtFullPath(string filename)
        {
            var keeperInDropboxFullPath = GetKeeper2018InDropboxPath();
            var dbPath = keeperInDropboxFullPath + @"\OldKeeperTxt";
            return Path.Combine(dbPath, filename);
        }

        public static string GetBackupFilePath(string filename)
        {
            var keeperInDropboxFullPath = GetKeeper2018InDropboxPath();
            var dbPath = keeperInDropboxFullPath + @"\Backup";
            return Path.Combine(dbPath, filename);
        }

        public static string GetDbFullPath()
        {
            var keeperInDropboxFullPath = GetKeeper2018InDropboxPath();
            return Path.Combine(keeperInDropboxFullPath, @"Db\KeeperDb.bin");
        }

        private static string GetKeeper2018InDropboxPath()
        {
            return Path.Combine(GetCurrentDropboxPath(), @"Keeper2018");
        }

        private static string GetCurrentDropboxPath()
        {
            const string infoPath = @"Dropbox\info.json";
            var jsonPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData") ?? throw new InvalidOperationException(), infoPath);
            if (!File.Exists(jsonPath)) jsonPath = Path.Combine(Environment.GetEnvironmentVariable("AppData") ?? throw new InvalidOperationException(), infoPath);
            if (!File.Exists(jsonPath)) throw new Exception("Dropbox could not be found!");
            var strings = File.ReadAllText(jsonPath).Split('\"');
            var index = strings.ToList().IndexOf("path");
            var dropboxPath = strings[index+2].Replace(@"\\", @"\");
            return dropboxPath;
        }
    }
}