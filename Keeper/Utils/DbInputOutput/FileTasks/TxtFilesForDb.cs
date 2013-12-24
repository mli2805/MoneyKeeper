using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.ZipTasks
{
  public static class TxtFilesForDb
  {
    public static Dictionary<string, int> Dict
    {
      get
      {
        return new Dictionary<string, int>
                 {
                   { "Accounts.txt", 215 },
                   { "ArticlesAssociations.txt", 225 },
                   { "CurrencyRates.txt", 235 },
                   { "Transactions.txt", 245 },
                 };
      }
    }
  }
}