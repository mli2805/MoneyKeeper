using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	[Export]
	public class DbAccountsWalker
	{
		public IEnumerable<HierarchyItem<Account>> Walk(IEnumerable<Account> accounts)
		{
			return accounts.SelectMany(accountsRoot => SaveAccount(accountsRoot, 0));
		}

		private static IEnumerable<HierarchyItem<Account>> SaveAccount(Account account, int depth)
		{
			yield return new HierarchyItem<Account>(depth, account);
			foreach (var hierarchyItem in account.Children.SelectMany(child => SaveAccount(child, depth + 1)))
				yield return hierarchyItem;
		}
	}
}