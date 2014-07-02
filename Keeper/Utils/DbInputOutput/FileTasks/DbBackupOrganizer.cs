using System.IO;
using System.Linq;

using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput.FileTasks
{
  public class DbBackupOrganizer
  {
    public void RemoveIdenticalBackups()
    {
        var backupFiles = Directory.EnumerateFiles(Settings.Default.KeeperFolder+Settings.Default.DbFolder, "DB*.zip").ToList();
      for (var i = 0; i < backupFiles.Count()-1; i++)
      {
        var file = new FileInfo(backupFiles[i]);
        var file2 = new FileInfo(backupFiles[i + 1]);
        if (file.Length == file2.Length)
        {
          File.Delete(backupFiles[i]);
        }
      }
    }
  }
}
