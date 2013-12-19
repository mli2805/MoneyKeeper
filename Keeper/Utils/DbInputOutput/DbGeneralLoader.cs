using System.Composition;
using System.IO;
using System.Windows;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;

namespace Keeper.Utils.DbInputOutput
{
  [Export]
  [Shared]
  internal class DbGeneralLoader
  {
    [Export]
    [Export(typeof(IKeeperDb))]
    public KeeperDb Db { get; private set; }

    readonly IMessageBoxer _messageBoxer;
    readonly IMyOpenFileDialog _openFileDialog;
    readonly IDbSerializer _dbSerializer;
    readonly IDbFromZipLoader _fromZipLoader;
    readonly IDbFromTxtLoader _fromTxtLoader;
    readonly IFileSystem _fileSystem;

    public DbLoadError LoadResult;

    [ImportingConstructor]
    public DbGeneralLoader(IMessageBoxer messageBoxer, IMyOpenFileDialog openFileDialog,
      IDbSerializer dbSerializer, IDbFromTxtLoader fromTxtLoader, IFileSystem fileSystem, IDbFromZipLoader fromZipLoader)
    {
      _messageBoxer = messageBoxer;
      _openFileDialog = openFileDialog;
      _dbSerializer = dbSerializer;
      _fromTxtLoader = fromTxtLoader;
      _fileSystem = fileSystem;
      _fromZipLoader = fromZipLoader;

      LoadResult = new DbLoadError { Code = 0 };

      Db = FullDbLoadProcess();
    }

    KeeperDb FullDbLoadProcess()
    {
      var filename = Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile);
      if (!_fileSystem.GetFile(filename).Exists)
      {
        if (!AskUserForAnotherFile(ref filename)) return null;
      }
      return LoadInAppropriateWay(filename);
    }

    private KeeperDb LoadInAppropriateWay(string filename)
    {
      var extension = Path.GetExtension(filename);

      if (extension == ".dbx")
      {
        var db = _dbSerializer.DecryptAndDeserialize(filename);
        if (db == null) LoadResult.Set(0x11, "Problem with dbx file!");
        return db;
      }
      if (extension == ".zip")
      {
        var db = new KeeperDb();
        LoadResult = _fromZipLoader.LoadDbFromZip(ref db, filename);
        return db;
      }
      if (extension == ".txt")
      {
        var db = new KeeperDb();
        LoadResult = _fromTxtLoader.LoadDbFromTxt(ref db, Path.GetDirectoryName(filename));
        return db;
      }
      return null;
    }

    private bool AskUserForAnotherFile(ref string filename)
    {
      _messageBoxer.Show("File '" + filename +
                         "' not found. \n\n You will be offered to choose database file.",
                         "Error!", MessageBoxButton.OK, MessageBoxImage.Warning);

      filename = _openFileDialog.Show("*.*",
                                      "All files (*.*)|*.*|Keeper Database (.dbx)|*.dbx|Zip archive (with keeper database .zip)|*.zip|Text files (with data for keeper .txt)|*.txt",
                                      Path.GetFullPath(Settings.Default.TestDbPath));
      if (filename == "")
      {
        LoadResult.Set(0x15, "Standart dbx file not found. User refused selection");
        return false;
      }

      Settings.Default.DbPath = Path.GetDirectoryName(filename);
      return true;
    }
  }
}
