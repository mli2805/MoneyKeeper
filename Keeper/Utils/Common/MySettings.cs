using System.Composition;
using Keeper.Properties;

namespace Keeper.Utils
{
  public interface IMySettings
  {
    object GetSetting(string name);
    void SetSetting(string name, object value);
    void Save();
  }

  [Export(typeof(IMySettings))]
  class MySettings : IMySettings
  {
    public object GetSetting(string name)
    {
      return Settings.Default[name];
    }

    public void SetSetting(string name, object value)
    {
      Settings.Default[name] = value;
    }

    public void Save() { Settings.Default.Save(); }
  }
}
