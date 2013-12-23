namespace Keeper.Utils.DbInputOutput
{
	public interface ILoader
	{
		string FileExtension { get; }
		DbLoadResult Load(string filename);
	}
}