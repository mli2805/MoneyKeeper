namespace Keeper.Utils.DbInputOutput
{
	public interface ILoader
	{
		string SupportedExtension { get; }
		DbLoadResult Load(string filename);
	}
}