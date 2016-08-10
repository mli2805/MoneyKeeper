using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    public interface IDbEntriesToStringListsConverter
    {
        IEnumerable<string> ConvertBankDepositOffersRatesToFileContent();
        IEnumerable<string> ConvertBankDepositOffersToFileContent();
        IEnumerable<string> ConvertTranWithTagsToFileContent();
        IEnumerable<string> ConvertArticlesAssociationsToFileContent();
        IEnumerable<string> ConvertCurrencyRatesToFileContent();
        IEnumerable<string> ConvertOfficialRatesToFileContent();
        IEnumerable<string> ConvertDepositsToFileContent();
        IEnumerable<string> ConvertAccountsToFileContent();
    }
}