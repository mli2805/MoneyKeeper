using System.Composition;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    [Export(typeof(IDbToTxtSaver))]
    public class DbToTxtSaver : IDbToTxtSaver
    {
        readonly IDbEntriesToStringListsConverter _dbEntriesToStringListsConverter;
        readonly IDbTxtFileWriter _dbTxtFileWriter;

        [ImportingConstructor]
        public DbToTxtSaver(IDbEntriesToStringListsConverter entriesToStringListsConverter, IDbTxtFileWriter dbTxtFileWriter)
        {
            _dbEntriesToStringListsConverter = entriesToStringListsConverter;
            _dbTxtFileWriter = dbTxtFileWriter;
        }

        public void SaveDbInTxt()
        {
            _dbTxtFileWriter.WriteDbFile("Accounts.txt", _dbEntriesToStringListsConverter.ConvertAccountsToFileContent());
            _dbTxtFileWriter.WriteDbFile("Transactions.txt", _dbEntriesToStringListsConverter.ConvertTransactionsToFileContent());
            _dbTxtFileWriter.WriteDbFile("TransWithTags.txt", _dbEntriesToStringListsConverter.ConvertTranWithTagsToFileContent());
            _dbTxtFileWriter.WriteDbFile("ArticlesAssociations.txt", _dbEntriesToStringListsConverter.ConvertArticlesAssociationsToFileContent());
            _dbTxtFileWriter.WriteDbFile("CurrencyRates.txt", _dbEntriesToStringListsConverter.ConvertCurrencyRatesToFileContent());
            _dbTxtFileWriter.WriteDbFile("OfficialRates.txt", _dbEntriesToStringListsConverter.ConvertOfficialRatesToFileContent());
            _dbTxtFileWriter.WriteDbFile("BankDepositOffers.txt", _dbEntriesToStringListsConverter.ConvertBankDepositOffersToFileContent());
            _dbTxtFileWriter.WriteDbFile("BankDepositOffersRates.txt", _dbEntriesToStringListsConverter.ConvertBankDepositOffersRatesToFileContent());
            _dbTxtFileWriter.WriteDbFile("Deposits.txt", _dbEntriesToStringListsConverter.ConvertDepositsToFileContent());
        }

    }
}
