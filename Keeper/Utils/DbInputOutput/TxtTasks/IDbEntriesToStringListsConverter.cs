using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    public interface IDbEntriesToStringListsConverter
    {
        IEnumerable<string> ConvertBankDepositOffersRatesToFileContent();
        IEnumerable<string> ConvertBankDepositOffersToFileContent();
        IEnumerable<string> ConvertTransactionsToFileContent();
        IEnumerable<string> ConvertArticlesAssociationsToFileContent();
        IEnumerable<string> ConvertCurrencyRatesToFileContent();
        IEnumerable<string> ConvertDepositsToFileContent();
        IEnumerable<string> ConvertAccountsToFileContent();
    }
}