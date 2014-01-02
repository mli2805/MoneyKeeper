using System.Composition;
using Keeper.Properties;

namespace Keeper.Utils
{
  public interface IMySettings
  {
    string DbxFile { get; set; }
    string TemporaryTxtDbPath { get; set; }
    string DbPath { get; set; }
    string KeeperInDropBox { get; set; }
    void Save();
  }

  [Export (typeof(IMySettings))]
  class MySettings : IMySettings
  {
    public string DbxFile 
    {
      get { return Settings.Default.DbxFile; }
      set { Settings.Default.DbxFile = value; } 
    }

    public string TemporaryTxtDbPath 
    { 
      get { return Settings.Default.TemporaryTxtDbPath; } 
      set { Settings.Default.TemporaryTxtDbPath = value;} 
    }

    public string DbPath 
    {
      get { return Settings.Default.DbPath; }
      set { Settings.Default.DbPath = value; } 
    }

    public string KeeperInDropBox 
    {
      get { return Settings.Default.KeeperInDropBox; }
      set { Settings.Default.KeeperInDropBox = value; } 
    }

    public void Save() { Settings.Default.Save();}
  }
}
