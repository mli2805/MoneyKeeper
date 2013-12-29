using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.DomainModel
{
  /// <summary>
	/// <see cref="DbGeneralLoader"/> is factory for the KeeperDb
	/// </summary>
	[Serializable]
	public class KeeperDb
	{
		public ObservableCollection<Account> Accounts { get; set; }
		public ObservableCollection<Transaction> Transactions { get; set; }
		public ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
		public ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }

    // Accounts - дерево счетов, а иногда нужен плоский список
    // В dbx хранится , а в текстовые файлы не выгружается и 
    // должен быть сформирован из дерева счетов в момент загрузки
    public List<Account> AccountsPlaneList { get; set; } 
	}
}
