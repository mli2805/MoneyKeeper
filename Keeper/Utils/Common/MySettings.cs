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

	    public string GetCombinedSetting(string name)
	    {
	        switch (name)
	        {
                case "DbFileFullPath" :
	                return (string) Settings.Default["KeeperFolder"] + (string) Settings.Default["DbFolder"] + (string) Settings.Default["DbxFile"];
                case "RegularPaymentsFileFullPath":
                    return (string)Settings.Default["KeeperFolder"] + (string)Settings.Default["DbFolder"] + (string)Settings.Default["RegularPaymentsFile"];
                case "BackupPath":
                    return (string)Settings.Default["KeeperFolder"] + (string)Settings.Default["BackupFolder"];
                default:
	                return "";
	        }
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
            yield return new OneSetting("DbFolder", (string)Settings.Default["DbFolder"]);
            yield return new OneSetting("KeeperFolder", (string)Settings.Default["KeeperFolder"]);
            yield return new OneSetting("BackupFolder", (string)Settings.Default["BackupFolder"]);
            yield return new OneSetting("ToDoFile", (string)Settings.Default["ToDoFile"]);
            yield return new OneSetting("RegularPaymentsFile", (string)Settings.Default["RegularPaymentsFile"]);
	        yield return new OneSetting("IgnoreMonthlyDepositProfitBelowByr", ((decimal)Settings.Default["IgnoreMonthlyDepositProfitBelowByr"]).ToString());
            yield return new OneSetting("LargeExpenseUsd", ((decimal)Settings.Default["LargeExpenseUsd"]).ToString());

	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }
	}
}
