using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	public interface IDbEntriesToStringListsConverter
	{
		IEnumerable<string> ConvertTransactionsToFileContent();
		IEnumerable<string> ConvertArticlesAssociationsToFileContent();
		IEnumerable<string> ConvertCurrencyRatesToFileContent();
		IEnumerable<string> ConvertDepositsRatesToFileContent();
		IEnumerable<string> ConvertDepositsToFileContent();
		IEnumerable<string> ConvertAccountsToFileContent();
	}
}