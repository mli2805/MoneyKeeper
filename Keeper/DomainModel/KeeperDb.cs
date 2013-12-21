using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Keeper.Utils.DbInputOutput;

namespace Keeper.DomainModel
{
  /// <summary>
	/// <see cref="DbGeneralLoader"/> is factory for the KeeperDb
	/// </summary>
	[Serializable]
	public class KeeperDb : IKeeperDb
	{
		public ObservableCollection<Account> Accounts { get; set; }
		public ObservableCollection<Transaction> Transactions { get; set; }
		public ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
		public ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }

    // не хранится , а формируется из дерева счетов в момент десериализации/загрузки из текстового файла
    public List<Account> AccountsPlaneList { get; set; } 

		// Accounts - дерево счетов, т.е. в базе счета хранятся в виде списка со ссылками на родительский счет
    // загружаются/десериализуются в виде дерева , а иногда нужен плоский список
		public static List<Account> FillInAccountsPlaneList(IEnumerable<Account> accountsList)
		{
			var result = new List<Account>();
			foreach (var account in accountsList)
			{
				result.Add(account);
				var childList = FillInAccountsPlaneList(account.Children);
				result.AddRange(childList);
			}
			return result;
		}

	}

//  public class AccountsPlaneListInitializer
}
