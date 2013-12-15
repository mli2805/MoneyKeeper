using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Keeper.DbInputOutput;

namespace Keeper.DomainModel
{
	public interface IKeeperDb {
		ObservableCollection<Account> Accounts { get; set; }
		ObservableCollection<Transaction> Transactions { get; set; }
		ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
		ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }
		List<Account> AccountsPlaneList { get; set; }
		Account FindAccountInBranch(string toFind, Account branch);
		Account FindAccountInTree(string name);
		int GetMaxAccountId();
	}
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

		public List<Account> AccountsPlaneList { get; set; } // не хранится , а формируется из дерева счетов в момент десериализации 

		// Accounts - дерево счетов, т.е. в базе счета хранятся в виде дерева и автоматически
		// сериализуются/десериализуются в виде дерева , а иногда нужен плоский список
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

		public Account FindAccountInBranch(string toFind, Account branch)
		{
			foreach (var child in branch.Children)
			{
				if (child.Name == toFind) return child;
				var res = FindAccountInBranch(toFind, child);
				if (res != null) return res;
			}
			return null;
		}

		public Account FindAccountInTree(string name)
		{
			foreach (var root in Accounts)
			{
				if (root.Name == name) return root;
				var res = FindAccountInBranch(name, root);
				if (res != null) return res;
			}
			return null;
		}

		public int GetMaxAccountId()
		{
			return (from account in AccountsPlaneList select account.Id).Max();
		}


	}
}
