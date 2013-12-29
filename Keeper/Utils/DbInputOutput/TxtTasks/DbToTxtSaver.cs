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
			mWriter.WriteDbFile("Accounts.txt", mEntriesToStringListsConverter.AccountsToList());
			mWriter.WriteDbFile("Transactions.txt", mEntriesToStringListsConverter.SaveTransactions());
			mWriter.WriteDbFile("ArticlesAssociations.txt", mEntriesToStringListsConverter.SaveArticlesAssociations());
			mWriter.WriteDbFile("CurrencyRates.txt", mEntriesToStringListsConverter.SaveCurrencyRates());
		}

	}
}
