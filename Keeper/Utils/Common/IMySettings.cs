namespace Keeper.Utils.Common
{
	public interface IMySettings
	{
		object GetSetting(string name);
        string GetCombinedSetting(string name);
		void SetSetting(string name, object value);
		void Save();
	}
}