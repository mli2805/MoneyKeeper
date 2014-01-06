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
			mWriter.WriteDbFile("Accounts.txt", mEntriesToStringListsConverter.ConvertAccountsToFileContent());
			mWriter.WriteDbFile("Transactions.txt", mEntriesToStringListsConverter.ConvertTransactionsToFileContent());
			mWriter.WriteDbFile("ArticlesAssociations.txt", mEntriesToStringListsConverter.ConvertArticlesAssociationsToFileContent());
			mWriter.WriteDbFile("CurrencyRates.txt", mEntriesToStringListsConverter.ConvertCurrencyRatesToFileContent());
		}

	}
}
