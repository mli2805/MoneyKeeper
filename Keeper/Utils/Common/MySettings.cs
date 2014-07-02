using System.Collections;
using System.Collections.Generic;
using System.Composition;

using Keeper.Properties;

namespace Keeper.Utils.Common
{
    public class OneSetting
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public OneSetting(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

	[Export(typeof(IMySettings))]
	[Export(typeof(MySettings))]
	public class MySettings : IMySettings, IEnumerable<OneSetting>
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

	    public IEnumerator<OneSetting> GetEnumerator()
	    {
	        yield return new OneSetting("DbxFile", (string)Settings.Default["DbxFile"]);
            yield return new OneSetting("TemporaryTxtDbPath", (string)Settings.Default["TemporaryTxtDbPath"]);
            yield return new OneSetting("DbPath", (string)Settings.Default["DbPath"]);
            yield return new OneSetting("KeeperInDropBox", (string)Settings.Default["KeeperInDropBox"]);
            yield return new OneSetting("OptionsFile", (string)Settings.Default["OptionsFile"]);
            yield return new OneSetting("BackupPath", (string)Settings.Default["BackupPath"]);
            yield return new OneSetting("ToDoFile", (string)Settings.Default["ToDoFile"]);
            yield return new OneSetting("RegularPaymentsFile", (string)Settings.Default["RegularPaymentsFile"]);
	        yield return new OneSetting("IgnoreMonthlyDepositProfitBelowByr", ((decimal)Settings.Default["IgnoreMonthlyDepositProfitBelowByr"]).ToString());

	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }
	}
}
