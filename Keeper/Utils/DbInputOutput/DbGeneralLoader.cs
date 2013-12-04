using System.IO;
using System.Windows;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.DbInputOutput
{
  class DbGeneralLoader
  {
    public KeeperDb FullDbLoadProcess()
    {
      var filename = Path.Combine(Settings.Default.SavePath, "Keeper.dbx");
      if (!File.Exists(filename))
      {
        MessageBox.Show("");
        MessageBox.Show("File '" + filename + "' not found. \n\n You will be offered to choose database file.", "Error!", MessageBoxButton.OK, MessageBoxImage.Warning);

        // Create OpenFileDialog
        // Set filter for file extension and default file extension
        var dialog = new Microsoft.Win32.OpenFileDialog { DefaultExt = ".dbx", Filter = "Keeper Database (.dbx)|*.dbx" };
        var result = dialog.ShowDialog();

        // Get the selected file name and display in a TextBox
        filename = result == true ? dialog.FileName : @"g:\local_keeperDb\Keeper.dbx";
      }
      var db = new DbSerializer().DecryptAndDeserialize(filename);
      if (db != null) return db;

      MessageBox.Show("");
      MessageBox.Show("File '" + filename + "' not found. \n Last zip will be used.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

      var loadResult = new DbFromTxtLoader().LoadFromLastZip(ref db);
      if (loadResult.Code == 0) return db;

      MessageBox.Show(loadResult.Explanation + ". \n Application will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
      return null;
    }

  }
}
