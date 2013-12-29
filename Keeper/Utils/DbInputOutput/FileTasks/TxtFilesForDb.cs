using System.Collections.Generic;

namespace Keeper.Utils.DbInputOutput.FileTasks
{
	public static class TxtFilesForDb
	{
		static readonly Dictionary<string, int> sDictionary = new Dictionary<string, int>
		  {
			  { "Accounts.txt", 215 },
			  { "ArticlesAssociations.txt", 225 },
			  { "CurrencyRates.txt", 235 },
			  { "Transactions.txt", 245 },
		  };

		public static Dictionary<string, int> Dict
		{
			get { return sDictionary; }
		}
	}
}