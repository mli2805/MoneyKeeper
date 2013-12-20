using System;
using System.Composition;
using System.IO;
using Ionic.Zip;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.Utils.DbInputOutput
{
  [Export (typeof(IDbFromZipLoader))]
  class DbFromZipLoader : IDbFromZipLoader
  {
    public DbLoadError LoadDbFromZip(ref KeeperDb db, string zipFile)
    {
      var result = UnzipArchive(zipFile);
      return result.Code != 0 ? result : new DbFromTxtLoader().LoadDbFromTxt(ref db, Settings.Default.TemporaryTxtDbPath);
    }

    private DbLoadError UnzipArchive(string zipFilename)
    {
      var unpackDirectory = Settings.Default.TemporaryTxtDbPath;
      using (var zipFile = ZipFile.Read(zipFilename))
      {
        // here, we extract every entry, but we could extract conditionally
        // based on entry name, size, date, checkbox status, etc.  
        foreach (ZipEntry innerFile in zipFile)
        {
          try
          {
            innerFile.ExtractWithPassword(unpackDirectory, ExtractExistingFileAction.OverwriteSilently, "!opa1526");
          }
          catch (Exception exception)
          {
            if (exception is BadPasswordException)
              return new DbLoadError{Code = 21, Explanation = "Bad password!"};
            throw;
          }
        }
      }
      if (!File.Exists(Path.Combine(Settings.Default.TemporaryTxtDbPath, "Accounts.txt")))
        return new DbLoadError {Code = 215, Explanation = "Accounts.txt not found!"};
      if (!File.Exists(Path.Combine(Settings.Default.TemporaryTxtDbPath, "ArticlesAssociations.txt")))
        return new DbLoadError {Code = 225, Explanation = "ArticlesAssociations.txt not found!"};
      if (!File.Exists(Path.Combine(Settings.Default.TemporaryTxtDbPath, "CurrencyRates.txt")))
        return new DbLoadError {Code = 235, Explanation = "CurrencyRates.txt not found!"};
      if (!File.Exists(Path.Combine(Settings.Default.TemporaryTxtDbPath, "Transactions.txt")))
        return new DbLoadError {Code = 245, Explanation = "Transactions.txt not found!"};
      return new DbLoadError();
    }

  }
}