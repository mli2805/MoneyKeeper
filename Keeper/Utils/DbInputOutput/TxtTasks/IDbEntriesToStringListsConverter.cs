using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	public interface IDbEntriesToStringListsConverter
	{
		IEnumerable<string> SaveTransactions();
		IEnumerable<string> SaveArticlesAssociations();
		IEnumerable<string> SaveCurrencyRates();
		IEnumerable<string> AccountsToList();
	}
}