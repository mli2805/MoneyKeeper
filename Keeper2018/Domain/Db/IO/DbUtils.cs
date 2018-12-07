using System;
using System.IO;
using Keeper2018.Properties;

namespace Keeper2018
{
    public static class DbUtils
    {
        // export from old Keeper
        public static string GetOldTxtFullPath(string filename)
        {
            var keeperInDropboxFullPath = GetKeeperInDropboxFullPath();
            var dbPath = keeperInDropboxFullPath + @"\OldKeeperTxt";
            return Path.Combine(dbPath, filename);
        }

        public static string GetDbFullPath()
        {
            var keeperInDropboxFullPath = GetKeeperInDropboxFullPath();
            var dbPath = keeperInDropboxFullPath + @"\Db";
            return Path.Combine(dbPath, "KeeperDb.bin");
        }

        private static string GetKeeperInDropboxFullPath()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = (string) Settings.Default["KeeperInDropbox"];
            return Path.Combine(path, dataFolder);
        }
    }
}