namespace Keeper.Utils.DbInputOutput
{
	public interface ILoader
	{
		string AssociatedExtension { get; }
		DbLoadResult Load(string filename);
	}
}