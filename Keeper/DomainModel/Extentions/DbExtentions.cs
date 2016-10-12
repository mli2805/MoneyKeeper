using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Common;

namespace Keeper.DomainModel.Extentions
{
    public static class DbExtentions
    {
        public static Account SeekAccount(this KeeperDb db, string name)
        {
            return db.FlattenAccounts().FirstOrDefault(a => a.Name == name);
        }

        public static IEnumerable<Account> FlattenAccounts(this KeeperDb db)
        {
            return db.FlattenAccountsWithLevels().Select(a => a.Item);
        }

        public static IEnumerable<HierarchyItem<Account>> FlattenAccountsWithLevels(this KeeperDb db)
        {
            return db.Accounts.SelectMany(accountsRoot => RecursiveWalk(accountsRoot, 0));
        }

        private static IEnumerable<HierarchyItem<Account>> RecursiveWalk(Account account, int depth)
        {
            yield return new HierarchyItem<Account>(depth, account);
            foreach (var hierarchyItem in account.Children.SelectMany(child => RecursiveWalk(child, depth + 1)))
                yield return hierarchyItem;
        }

        public static AccName FindThroughTheForest(this List<AccName> roots, string name)
        {
            foreach (var root in roots)
            {
                var result = root.FindThroughTree(name);
                if (result != null) return result;
            }
            return null;
        }

    }
}
