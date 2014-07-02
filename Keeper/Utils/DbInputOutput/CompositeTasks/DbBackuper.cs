using System;
using System.Composition;
using System.IO;
using System.Windows;
using Ionic.Zip;
using Keeper.Properties;
using Keeper.Utils.Common;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Keeper.Utils.DbInputOutput.CompositeTasks
{
	[Export]
  public class DbBackuper : IDbBackuper
  {
    private readonly IDbToTxtSaver _txtSaver;
	    private readonly MySettings _mySettings;

	    [ImportingConstructor]
    public DbBackuper(IDbToTxtSaver txtSaver, MySettings mySettings)
    {
        _txtSaver = txtSaver;
        _mySettings = mySettings;
    }

	    public void MakeDbBackupCopy()
    {
      _txtSaver.SaveDbInTxt();
      ZipTxtDb();
      DeleteTxtDb();
    }

    public void DeleteTxtDb()
    {
      if (!Directory.Exists(Settings.Default.TemporaryTxtDbPath)) return;
      var filenames = Directory.GetFiles((string)_mySettings.GetSetting("TemporaryTxtDbPath"), "*.txt"); // note: this does not recurse directories! 
      foreach (var filename in filenames)
      {
        File.Delete(filename);
      }
    }

    public void ZipTxtDb()
    {
      var archiveName = String.Format("DB{0:yyyy-MM-dd-HH-mm-ss}.zip", DateTime.Now);
        var zipFileToCreate = Path.Combine(_mySettings.GetCombinedSetting("BackupPath"), archiveName);
        var directoryToZip = (string)_mySettings.GetSetting("TemporaryTxtDbPath");
      try
      {
        using (var zip = new ZipFile())
        {
          zip.Password = "!opa1526";
          zip.Encryption = EncryptionAlgorithm.WinZipAes256;
          var filenames = Directory.GetFiles(directoryToZip, "*.txt"); // note: this does not recurse directories! 
          foreach (var filename in filenames)
            zip.AddFile(filename, String.Empty);
          zip.Comment = String.Format("This zip archive was created  on machine '{0}'", System.Net.Dns.GetHostName());
          zip.Save(zipFileToCreate);
        }
      }
      catch (Exception ex1)
      {
        MessageBox.Show("Exception during database zipping: " + ex1);
      }
    }
 
  }
}