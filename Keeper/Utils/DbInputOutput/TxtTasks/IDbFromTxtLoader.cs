using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	public interface IDbFromTxtLoader
	{
		DbLoadResult LoadDbFromTxt(string path);
	}
}