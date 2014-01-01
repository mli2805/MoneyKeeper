using System;
using System.Collections.ObjectModel;
using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.DomainModel
{
  /// <summary>
  /// <see cref="DbExporter"/> is factory for the KeeperDb
	/// </summary>
	[Serializable]
	public class KeeperDb
	{
		public ObservableCollection<Account> Accounts { get; set; }
		public ObservableCollection<Transaction> Transactions { get; set; }
		public ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
		public ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }
	}
}
