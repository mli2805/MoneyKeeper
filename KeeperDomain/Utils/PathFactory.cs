using System.IO;

namespace KeeperDomain
{
    public static class PathFactory
    {
        // export from old Keeper
        public static string GetOldTxtFullPath(string filename)
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            var dbPath = keeperInDropboxFullPath + @"\OldKeeperTxt";
            return Path.Combine(dbPath, filename);
        }

        public static string GetBackupFilePath(string filename)
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            var dbPath = keeperInDropboxFullPath + @"\Backup";
            return Path.Combine(dbPath, filename);
        }

        public static string GetReportFullPath(string filename)
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            var dbPath = keeperInDropboxFullPath + @"\Reports";
            return Path.Combine(dbPath, filename);
        }

        public static string GetBackupPath()
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            return keeperInDropboxFullPath + @"\Backup";
        }

        public static string GetDbFullPath()
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            return Path.Combine(keeperInDropboxFullPath, @"Db\KeeperDb.bin");
        }

    }
}