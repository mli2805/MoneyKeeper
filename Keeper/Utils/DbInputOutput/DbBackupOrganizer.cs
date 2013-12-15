using System.IO;
using System.Linq;
using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput
{
  class DbBackupOrganizer
  {
    public void RemoveIdenticalBackups()
    {
      var backupFiles = Directory.EnumerateFiles(Settings.Default.SavePath,"DB*.zip").ToList();
      for (int i = 0; i < backupFiles.Count()-1; i++)
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
