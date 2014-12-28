using System.Collections.Generic;

using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.Utils.DbInputOutput.FileTasks
{
	public interface IFileExistenceChecker {
		DbLoadResult Check(Dictionary<string, int> files);
	}
}