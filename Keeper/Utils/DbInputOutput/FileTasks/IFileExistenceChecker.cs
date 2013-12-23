using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput
{
	public interface IFileExistenceChecker {
		DbLoadResult Check(Dictionary<string, int> files);
	}
}