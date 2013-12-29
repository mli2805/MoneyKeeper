using System.Composition;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	[Export]
	public class DbToTxtSaver : IDbToTxtSaver
	{
		readonly IDbEntriesToStringListsConverter mEntriesToStringListsConverter;
		readonly DbTxtFileWriter mWriter;

		[ImportingConstructor]
		public DbToTxtSaver(IDbEntriesToStringListsConverter entriesToStringListsConverter, DbTxtFileWriter writer)
		{
			mEntriesToStringListsConverter = entriesToStringListsConverter;
			mWriter = writer;
		}

		public void SaveDbInTxt()
		{
			mWriter.GetValue("Accounts.txt", mEntriesToStringListsConverter.AccountsToList());
			mWriter.GetValue("Transactions.txt", mEntriesToStringListsConverter.SaveTransactions());
			mWriter.GetValue("ArticlesAssociations.txt", mEntriesToStringListsConverter.SaveArticlesAssociations());
			mWriter.GetValue("CurrencyRates.txt", mEntriesToStringListsConverter.SaveCurrencyRates());
		}

	}
}
