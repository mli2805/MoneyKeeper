using System.IO;
using System.Windows;
using Keeper.Properties;

namespace Keeper.DbInputOutput
{
  class DbGeneralLoading
  {
    public static bool FullDbLoadProcess()
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
      if (BinaryCrypto.DbCryptoDeserialization(filename)) return true;

      MessageBox.Show("");
      MessageBox.Show("File '" + filename + "' not found. \n Last zip will be used.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

      var loadResult = DbTxtLoad.LoadFromLastZip();
      if (loadResult.Code == 0) return true;

      MessageBox.Show(loadResult.Explanation + ". \n Application will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
      return false;
    }

  }
}
