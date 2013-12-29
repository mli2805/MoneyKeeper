namespace Keeper.Utils.DbInputOutput.CompositeTasks
{
	public interface ILoader
	{
		string FileExtension { get; }
		DbLoadResult Load(string filename);
	}
}