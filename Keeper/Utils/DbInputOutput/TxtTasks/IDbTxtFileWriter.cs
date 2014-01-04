using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  public interface IDbTxtFileWriter
  {
    void WriteDbFile(string accountsTxt, IEnumerable<string> accountsToList);
  }
}