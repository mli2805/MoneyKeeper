using System;
using System.IO;
using System.Linq;
using Keeper.DomainModel.WorkTypes;
using Keeper.Properties;
using Keeper.Utils.Dialogs;

namespace Keeper.Utils.DbInputOutput.FileTasks
{
    public class DbBackupOrganizer
    {
        const string FILTERS = "All files (*.*)|*.*|" +
          "Keeper Database (.dbx)|*.dbx|" +
          "Zip archive (with keeper database .zip)|*.zip|" +
          "Text files (with data for keeper .txt)|*.txt";

        public void RemoveIdenticalBackups()
        {
            var backupFiles = Directory.EnumerateFiles(Settings.Default.KeeperFolder + Settings.Default.BackupFolder, "DB*.zip").ToList();
            for (var i = 0; i < backupFiles.Count() - 1; i++)
            {
                var file = new FileInfo(backupFiles[i]);
                var file2 = new FileInfo(backupFiles[i + 1]);
                if (file.Length == file2.Length)
                {
                    File.Delete(backupFiles[i]);
                }
            }
        }

        public void RemoveAllNonFirstInMonth()
        {
            var yearFolder = ChooseFolder();
            if (yearFolder == null) return;

            var backupFiles = Directory.EnumerateFiles(yearFolder, "DB*.zip").ToList();
            for (var i = 0; i < backupFiles.Count() - 1; i++)
            {
                var ym1 = GetYearMonthFromArchieveName(backupFiles[i]);
                var ym2 = GetYearMonthFromArchieveName(backupFiles[i + 1]);
                if (ym1.CompareTo(ym2) == 0)
                {
                    File.Delete(backupFiles[i+1]);
                }
            }
        }

        private YearMonth GetYearMonthFromArchieveName(string fullFilename)
        {
            var filename = Path.GetFileName(fullFilename);
            var lines = filename.Split('-');
            return new YearMonth(Int16.Parse(lines[0].Substring(2,4)), Int16.Parse(lines[1]));
        }

        private string ChooseFolder()
        {
            IMyOpenFileDialog myOpenFileDialog = new MyOpenFileDialog();
            var another = myOpenFileDialog.Show("*.*", FILTERS, "");

            if (string.IsNullOrEmpty(another)) return null;
            return Path.GetDirectoryName(another);
        }
    }
}
