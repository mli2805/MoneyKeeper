using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	interface IDbFromTxtLoader
	{
		DbLoadResult LoadDbFromTxt(string path);
	}
}