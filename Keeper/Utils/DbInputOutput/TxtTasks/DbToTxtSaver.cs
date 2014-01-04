using System.Composition;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	[Export (typeof( IDbToTxtSaver))]
	public class DbToTxtSaver : IDbToTxtSaver
	{
		readonly IDbEntriesToStringListsConverter mEntriesToStringListsConverter;
		readonly IDbTxtFileWriter mWriter;

		[ImportingConstructor]
		public DbToTxtSaver(IDbEntriesToStringListsConverter entriesToStringListsConverter, IDbTxtFileWriter writer)
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
