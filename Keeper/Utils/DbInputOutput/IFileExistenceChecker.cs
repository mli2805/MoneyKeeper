using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput
{
	public interface IFileExistenceChecker {
		DbLoadError Check(Dictionary<string, int> files);
	}
}