using System.Composition;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput
{
  [Export (typeof(IDbFromZipLoader))]
  class DbFromZipLoader : IDbFromZipLoader
  {
    public DbLoadError LoadDbFromZip(ref KeeperDb db, string filename)
    {
      // TODO здесь надо распаковать его ну и вызвать загрузку из текстовых
      return new DbFromTxtLoader().LoadDbFromTxt(ref db, filename);
    }

    private string GetLatestDbArchive()
    {
      return "Db.zip";
    }

    private void UnzipAllTables()
    {
      var zipToUnpack = GetLatestDbArchive();
      var unpackDirectory = Settings.Default.DbPath;
      using (var zip1 = ZipFile.Read(zipToUnpack))
      {
        // here, we extract every entry, but we could extract conditionally
        // based on entry name, size, date, checkbox status, etc.  
        foreach (ZipEntry e in zip1)
        {
          e.ExtractWithPassword(unpackDirectory, ExtractExistingFileAction.OverwriteSilently, "!opa1526");
        }
      }
    }

  }
}